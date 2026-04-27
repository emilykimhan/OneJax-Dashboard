using System.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;

namespace OneJaxDashboard.Services;

public class SqlServerInsertCompatibilityService
{
    private static readonly Regex SafeIdentifierPattern = new("^[A-Za-z0-9_]+$", RegexOptions.Compiled);
    private readonly ApplicationDbContext _context;

    public SqlServerInsertCompatibilityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void PrepareForInsert<TEntity>(TEntity entity, string tableName) where TEntity : class
    {
        if (!_context.Database.IsSqlServer())
        {
            return;
        }

        if (!RequiresExplicitIdInsert(tableName))
        {
            return;
        }

        var idProperty = typeof(TEntity).GetProperty("Id");
        if (idProperty == null || idProperty.PropertyType != typeof(int) || !idProperty.CanWrite)
        {
            return;
        }

        var currentId = (int?)idProperty.GetValue(entity) ?? 0;
        if (currentId != 0)
        {
            return;
        }

        idProperty.SetValue(entity, GetNextSqlServerId(tableName));
    }

    private bool RequiresExplicitIdInsert(string tableName)
    {
        var safeTableName = GetSafeIdentifier(tableName);
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT ISNULL(COLUMNPROPERTY(OBJECT_ID(N'{safeTableName}'), N'Id', 'IsIdentity'), -1)";
            var identityFlag = Convert.ToInt32(command.ExecuteScalar() ?? -1);
            return identityFlag == 0;
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }

    private int GetNextSqlServerId(string tableName)
    {
        var safeTableName = GetSafeIdentifier(tableName);
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT ISNULL(MAX([Id]), 0) + 1 FROM [{safeTableName}]";
            return Convert.ToInt32(command.ExecuteScalar() ?? 1);
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }

    private static string GetSafeIdentifier(string identifier)
    {
        if (!SafeIdentifierPattern.IsMatch(identifier))
        {
            throw new InvalidOperationException($"'{identifier}' is not a valid SQL Server identifier.");
        }

        return identifier;
    }
}
