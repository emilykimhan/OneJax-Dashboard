using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace OneJaxDashboard.Data;

public sealed class SqliteToSqlServerMigrator
{
    private readonly string _sourceConnectionString;
    private readonly string _targetConnectionString;
    private readonly Action<string> _log;

    public SqliteToSqlServerMigrator(
        string sourceConnectionString,
        string targetConnectionString,
        Action<string>? log = null)
    {
        _sourceConnectionString = sourceConnectionString;
        _targetConnectionString = targetConnectionString;
        _log = log ?? (_ => { });
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var sourceOptionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        DatabaseConfiguration.Configure(
            sourceOptionsBuilder,
            new DatabaseSettings(DatabaseProvider.Sqlite, _sourceConnectionString, InitializeSchemaOnStartup: false));

        var targetOptionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        DatabaseConfiguration.Configure(
            targetOptionsBuilder,
            new DatabaseSettings(DatabaseProvider.SqlServer, _targetConnectionString, InitializeSchemaOnStartup: false));

        await using var sourceContext = new ApplicationDbContext(sourceOptionsBuilder.Options);
        await using var targetContext = new ApplicationDbContext(targetOptionsBuilder.Options);

        _log("Applying any pending SQLite migrations to the source database...");
        await sourceContext.Database.MigrateAsync(cancellationToken);

        _log("Ensuring the SQL Server target schema exists...");
        await targetContext.Database.EnsureCreatedAsync(cancellationToken);

        var tables = GetOrderedTables(targetContext.Model);

        await using var sourceConnection = new SqliteConnection(_sourceConnectionString);
        await sourceConnection.OpenAsync(cancellationToken);

        await using var targetConnection = new SqlConnection(_targetConnectionString);
        await targetConnection.OpenAsync(cancellationToken);

        await EnsureTargetIsEmptyAsync(targetConnection, tables, cancellationToken);

        await using var transaction = (SqlTransaction)await targetConnection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var table in tables)
            {
                var rowCount = await GetSourceRowCountAsync(sourceConnection, table, cancellationToken);
                if (rowCount == 0)
                {
                    continue;
                }

                _log($"Copying {rowCount} row(s) into {table.TableName}...");
                var dataTable = await LoadSourceDataAsync(sourceConnection, table, cancellationToken);

                if (dataTable.Rows.Count == 0)
                {
                    continue;
                }

                var useIdentityInsert = RequiresIdentityInsert(table);
                if (useIdentityInsert)
                {
                    await ExecuteNonQueryAsync(
                        targetConnection,
                        transaction,
                        $"SET IDENTITY_INSERT {table.GetQualifiedTargetTableName()} ON;",
                        cancellationToken);
                }

                try
                {
                    using var bulkCopy = new SqlBulkCopy(
                        targetConnection,
                        SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.KeepIdentity,
                        transaction);

                    bulkCopy.DestinationTableName = table.GetQualifiedTargetTableName();

                    foreach (DataColumn column in dataTable.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }

                    await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
                }
                finally
                {
                    if (useIdentityInsert)
                    {
                        await ExecuteNonQueryAsync(
                            targetConnection,
                            transaction,
                            $"SET IDENTITY_INSERT {table.GetQualifiedTargetTableName()} OFF;",
                            cancellationToken);
                    }
                }
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task EnsureTargetIsEmptyAsync(
        SqlConnection targetConnection,
        IReadOnlyList<TableTransferPlan> tables,
        CancellationToken cancellationToken)
    {
        foreach (var table in tables)
        {
            await using var command = targetConnection.CreateCommand();
            command.CommandText = $"SELECT COUNT_BIG(1) FROM {table.GetQualifiedTargetTableName()};";
            var count = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));

            if (count > 0)
            {
                throw new InvalidOperationException(
                    $"Target table '{table.TableName}' already contains data. Use an empty SQL Server database for the migration.");
            }
        }
    }

    private static async Task<int> GetSourceRowCountAsync(
        SqliteConnection connection,
        TableTransferPlan table,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {table.GetQualifiedSourceTableName()};";
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    private static async Task<DataTable> LoadSourceDataAsync(
        SqliteConnection connection,
        TableTransferPlan table,
        CancellationToken cancellationToken)
    {
        var dataTable = new DataTable(table.TableName);

        foreach (var column in table.Columns)
        {
            dataTable.Columns.Add(column.ColumnName, GetDataColumnType(column.Property.ClrType));
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT {string.Join(", ", table.Columns.Select(column => $"\"{column.ColumnName}\""))}
            FROM {table.GetQualifiedSourceTableName()};
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var ordinals = table.Columns.ToDictionary(
            column => column.ColumnName,
            column => reader.GetOrdinal(column.ColumnName),
            StringComparer.OrdinalIgnoreCase);

        while (await reader.ReadAsync(cancellationToken))
        {
            var row = dataTable.NewRow();

            foreach (var column in table.Columns)
            {
                var value = reader.GetValue(ordinals[column.ColumnName]);
                row[column.ColumnName] = ConvertValue(value, column.Property.ClrType);
            }

            dataTable.Rows.Add(row);
        }

        return dataTable;
    }

    private static object ConvertValue(object value, Type targetType)
    {
        if (value == DBNull.Value)
        {
            return DBNull.Value;
        }

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(string))
        {
            return Convert.ToString(value) ?? string.Empty;
        }

        if (underlyingType == typeof(DateTime))
        {
            if (value is DateTime dateTime)
            {
                return dateTime;
            }

            return DateTime.Parse(Convert.ToString(value) ?? string.Empty);
        }

        if (underlyingType == typeof(bool))
        {
            return value switch
            {
                bool boolValue => boolValue,
                long longValue => longValue != 0,
                int intValue => intValue != 0,
                string stringValue when long.TryParse(stringValue, out var parsedLong) => parsedLong != 0,
                string stringValue when bool.TryParse(stringValue, out var parsedBool) => parsedBool,
                _ => Convert.ToBoolean(value)
            };
        }

        if (underlyingType == typeof(decimal))
        {
            return value switch
            {
                decimal decimalValue => decimalValue,
                double doubleValue => Convert.ToDecimal(doubleValue),
                float floatValue => Convert.ToDecimal(floatValue),
                long longValue => Convert.ToDecimal(longValue),
                int intValue => Convert.ToDecimal(intValue),
                string stringValue => decimal.Parse(stringValue),
                _ => Convert.ToDecimal(value)
            };
        }

        if (underlyingType.IsEnum)
        {
            return Enum.Parse(underlyingType, Convert.ToString(value) ?? string.Empty);
        }

        return Convert.ChangeType(value, underlyingType);
    }

    private static Type GetDataColumnType(Type type) =>
        Nullable.GetUnderlyingType(type) ?? type;

    private static bool RequiresIdentityInsert(TableTransferPlan table)
    {
        var primaryKey = table.EntityType.FindPrimaryKey();
        if (primaryKey == null || primaryKey.Properties.Count != 1)
        {
            return false;
        }

        var keyProperty = primaryKey.Properties[0];
        var keyColumnName = keyProperty.GetColumnName(StoreObjectIdentifier.Table(table.TableName, table.Schema));
        return keyColumnName != null &&
               table.Columns.Any(column => string.Equals(column.ColumnName, keyColumnName, StringComparison.OrdinalIgnoreCase)) &&
               keyProperty.ValueGenerated == ValueGenerated.OnAdd &&
               (keyProperty.ClrType == typeof(int) || keyProperty.ClrType == typeof(long));
    }

    private static IReadOnlyList<TableTransferPlan> GetOrderedTables(IModel model)
    {
        var storeObjectTables = model.GetEntityTypes()
            .Where(entityType => entityType.GetTableName() != null)
            .Select(entityType => new
            {
                EntityType = entityType,
                TableName = entityType.GetTableName()!,
                Schema = entityType.GetSchema()
            })
            .GroupBy(entry => new { entry.TableName, entry.Schema })
            .Select(group =>
            {
                var entityType = group.First().EntityType;
                var storeObject = StoreObjectIdentifier.Table(group.Key.TableName, group.Key.Schema);
                var columns = entityType.GetProperties()
                    .Select(property => new TableColumnPlan(
                        property.GetColumnName(storeObject) ?? property.Name,
                        property))
                    .Where(column => column.ColumnName != null)
                    .DistinctBy(column => column.ColumnName, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                return new TableTransferPlan(group.Key.TableName, group.Key.Schema, entityType, columns);
            })
            .ToDictionary(plan => (plan.TableName, plan.Schema), plan => plan);

        var dependencies = storeObjectTables.Values.ToDictionary(
            plan => plan,
            _ => new HashSet<TableTransferPlan>());

        foreach (var plan in storeObjectTables.Values)
        {
            foreach (var foreignKey in plan.EntityType.GetForeignKeys())
            {
                var principalTableName = foreignKey.PrincipalEntityType.GetTableName();
                if (principalTableName == null)
                {
                    continue;
                }

                var principalSchema = foreignKey.PrincipalEntityType.GetSchema();
                if (!storeObjectTables.TryGetValue((principalTableName, principalSchema), out var principalPlan))
                {
                    continue;
                }

                if (!ReferenceEquals(plan, principalPlan))
                {
                    dependencies[plan].Add(principalPlan);
                }
            }
        }

        var ordered = new List<TableTransferPlan>();
        var remaining = dependencies.ToDictionary(entry => entry.Key, entry => new HashSet<TableTransferPlan>(entry.Value));

        while (remaining.Count > 0)
        {
            var next = remaining
                .Where(entry => entry.Value.Count == 0)
                .Select(entry => entry.Key)
                .OrderBy(plan => plan.TableName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (next == null)
            {
                throw new InvalidOperationException("Unable to determine a safe table copy order for the database migration.");
            }

            ordered.Add(next);
            remaining.Remove(next);

            foreach (var dependencySet in remaining.Values)
            {
                dependencySet.Remove(next);
            }
        }

        return ordered;
    }

    private static async Task ExecuteNonQueryAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private sealed record TableTransferPlan(
        string TableName,
        string? Schema,
        IEntityType EntityType,
        IReadOnlyList<TableColumnPlan> Columns)
    {
        public string GetQualifiedSourceTableName() => $"\"{TableName}\"";

        public string GetQualifiedTargetTableName() =>
            string.IsNullOrWhiteSpace(Schema)
                ? $"[{TableName}]"
                : $"[{Schema}].[{TableName}]";
    }

    private sealed record TableColumnPlan(string ColumnName, IProperty Property);
}
