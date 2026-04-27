using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
using System.Security.Claims;
using System.Data;
using System.Data.Common;

namespace OneJaxDashboard.Controllers;

public class ProgramsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ActivityLogService _activityLog;
    private readonly EventsService _events;

    private static readonly string[] ProgramTypes =
    {
        "Humanitarian Awards",
        "Fundraising",
        "Youth",
        "Interfaith",
        "Community",
        "Donor"
    };

    public ProgramsController(ApplicationDbContext context, ActivityLogService activityLog, EventsService events)
    {
        _context = context;
        _activityLog = activityLog;
        _events = events;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var programs = _context.Programs
            .OrderByDescending(p => p.Id)
            .ToList();

        ViewBag.ProgramTypes = ProgramTypes;
        return View(programs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(string programName, string programType, string? description)
    {
        if (string.IsNullOrWhiteSpace(programName) || string.IsNullOrWhiteSpace(programType))
        {
            TempData["ProgramsError"] = "Program Name and Program Type are required.";
            return RedirectToAction(nameof(Index));
        }

        var program = new Programs
        {
            ProgramName = programName.Trim(),
            ProgramType = programType.Trim(),
            Description = (description ?? string.Empty).Trim()
        };

        try
        {
            PersistProgram(program);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[program-add] Failed to create program '{program.ProgramName}': {ex}");
            TempData["ProgramsError"] = "We couldn't save that program right now. Please try again.";
            return RedirectToAction(nameof(Index));
        }

        _activityLog.Log(GetActorName(), "Created Program", "Program",
            details: $"Id={program.Id}; Created '{program.ProgramName}' ({program.ProgramType})");

        TempData["ProgramsSuccess"] = "Program added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var program = _context.Programs.FirstOrDefault(p => p.Id == id);
        if (program == null)
        {
            return NotFound();
        }

        ViewBag.ProgramTypes = ProgramTypes;
        return View(program);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, string programName, string programType, string? description)
    {
        var program = _context.Programs.FirstOrDefault(p => p.Id == id);
        if (program == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(programName) || string.IsNullOrWhiteSpace(programType))
        {
            TempData["ProgramsError"] = "Program Name and Program Type are required.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var previousName = program.ProgramName;
        var previousType = program.ProgramType;
        var previousDescription = program.Description;
        var nextName = programName.Trim();
        var nextType = programType.Trim();
        var nextDescription = (description ?? string.Empty).Trim();

        program.ProgramName = nextName;
        program.ProgramType = nextType;
        program.Description = nextDescription;

        _context.SaveChanges();
        var changes = new List<string>();
        AddChange(changes, "Program Name", previousName, nextName);
        AddChange(changes, "Program Type", previousType, nextType);
        AddChange(changes, "Description", previousDescription, nextDescription);
        var changeDetails = changes.Count > 0 ? string.Join("; ", changes) : "No field changes detected";

        _activityLog.Log(
            GetActorName(),
            "Updated Program",
            "Program",
            details: $"Id={program.Id}; Updated '{program.ProgramName}'. Changes: {changeDetails}");

        TempData["ProgramsSuccess"] = "Program updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Archive(int id)
    {
        var program = _context.Programs.FirstOrDefault(p => p.Id == id);
        if (program != null)
        {
            try
            {
                var archivedProgram = new ArchivedProgram
                {
                    OriginalProgramId = program.Id,
                    ProgramName = program.ProgramName,
                    ProgramType = program.ProgramType,
                    Description = program.Description,
                    ArchivedAtUtc = DateTime.UtcNow
                };

                using var transaction = _context.Database.BeginTransaction();
                DetachProgramFromStrategies(program.Id);
                PersistArchivedProgram(archivedProgram);
                _context.Programs.Remove(program);
                _context.SaveChanges();
                transaction.Commit();

                _activityLog.Log(
                    GetActorName(),
                    "Archived Program",
                    "Program",
                    details: $"Id={program.Id}; Archived '{program.ProgramName}' ({program.ProgramType})");
                TempData["ProgramsSuccess"] = "Program archived successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[program-archive] Failed to archive program '{program.ProgramName}' (Id={program.Id}): {ex}");
                TempData["ProgramsError"] = "We couldn't archive that program right now. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        return RedirectToAction(nameof(Archive));
    }

    [HttpGet]
    public IActionResult Archive()
    {
        List<ArchivedProgram> programs;
        try
        {
            programs = LoadArchivedProgramsForDisplay();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[program-archive-view] Failed to load archived programs: {ex}");
            programs = new List<ArchivedProgram>();
            TempData["ProgramsError"] = "Archived programs couldn't be loaded right now.";
        }

        List<Strategy> events;
        try
        {
            events = LoadArchivedEventsForDisplay();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[program-archive-view] Failed to load archived events: {ex}");
            events = new List<Strategy>();
            var existingError = TempData["ProgramsError"] as string;
            TempData["ProgramsError"] = string.IsNullOrWhiteSpace(existingError)
                ? "Archived events couldn't be loaded right now."
                : $"{existingError} Archived events couldn't be loaded right now.";
        }

        var model = new ProgramArchiveViewModel
        {
            Programs = programs,
            Events = events
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Restore(int id)
    {
        var archivedProgram = _context.ArchivedPrograms.FirstOrDefault(p => p.Id == id);
        if (archivedProgram != null)
        {
            try
            {
                var restoredProgram = new Programs
                {
                    ProgramName = archivedProgram.ProgramName,
                    ProgramType = archivedProgram.ProgramType,
                    Description = archivedProgram.Description
                };

                PersistProgram(restoredProgram);
                _context.ArchivedPrograms.Remove(archivedProgram);
                _context.SaveChanges();

                _activityLog.Log(
                    GetActorName(),
                    "Restored Program",
                    "Program",
                    details: $"ArchivedId={archivedProgram.Id}; Restored '{restoredProgram.ProgramName}' ({restoredProgram.ProgramType}) as Id={restoredProgram.Id}");
                TempData["ProgramsSuccess"] = "Program restored successfully.";
            }
            catch (Exception)
            {
                TempData["ProgramsError"] = "We couldn't restore that program right now. Please try again.";
            }
        }

        return RedirectToAction(nameof(Archive));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RestoreEvent(int id)
    {
        try
        {
            if (_context.Database.IsSqlServer())
            {
                UnarchiveStrategyById(id);
            }
            else
            {
                var archivedEvent = _context.Strategies.FirstOrDefault(s => s.Id == id && s.IsArchived);
                if (archivedEvent == null)
                {
                    return RedirectToAction(nameof(Archive));
                }

                archivedEvent.IsArchived = false;
                archivedEvent.ArchivedAtUtc = null;
                _context.SaveChanges();
            }

            _events.UnarchiveByStrategyTemplate(id);
            TempData["ProgramsSuccess"] = "Event restored.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[program-restore-event] Failed to restore archived event Id={id}: {ex}");
            TempData["ProgramsError"] = "We couldn't restore that event right now. Please try again.";
        }

        return RedirectToAction(nameof(Archive));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        var program = _context.Programs.FirstOrDefault(p => p.Id == id);
        if (program != null)
        {
            var deletedName = program.ProgramName;
            var deletedType = program.ProgramType;

            _context.Programs.Remove(program);
            _context.SaveChanges();
            _activityLog.Log(GetActorName(), "Deleted Program", "Program",
                details: $"Id={id}; Deleted '{deletedName}' ({deletedType})");
            TempData["ProgramsSuccess"] = "Program deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private string GetActorName()
    {
        var username = User.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(username))
        {
            return username;
        }

        var claimName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
        if (!string.IsNullOrWhiteSpace(claimName))
        {
            return claimName;
        }

        return "System";
    }

    private static void AddChange(List<string> changes, string fieldName, string? before, string? after)
    {
        var oldValue = Normalize(before);
        var newValue = Normalize(after);
        if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
        {
            return;
        }

        changes.Add($"{fieldName}: '{Display(oldValue)}' -> '{Display(newValue)}'");
    }

    private static string Normalize(string? value) => (value ?? string.Empty).Trim();

    private static string Display(string value) => string.IsNullOrEmpty(value) ? "(empty)" : value;

    private void DetachProgramFromStrategies(int programId)
    {
        _context.Database.ExecuteSqlInterpolated($"""
            UPDATE [Strategies]
            SET [ProgramId] = NULL
            WHERE [ProgramId] = {programId};
            """);
    }

    private void PersistProgram(Programs program)
    {
        if (!RequiresExplicitIdInsert("Programs"))
        {
            _context.Programs.Add(program);
            _context.SaveChanges();
            return;
        }

        program.Id = GetNextSqlServerId("Programs");

        _context.Database.ExecuteSqlInterpolated($"""
            INSERT INTO [Programs] ([Id], [ProgramName], [Description], [ProgramType])
            VALUES ({program.Id}, {program.ProgramName}, {program.Description}, {program.ProgramType});
            """);
    }

    private void PersistArchivedProgram(ArchivedProgram archivedProgram)
    {
        if (!RequiresExplicitIdInsert("ArchivedPrograms"))
        {
            _context.ArchivedPrograms.Add(archivedProgram);
            _context.SaveChanges();
            return;
        }

        archivedProgram.Id = GetNextSqlServerId("ArchivedPrograms");

        _context.Database.ExecuteSqlInterpolated($"""
            INSERT INTO [ArchivedPrograms] ([Id], [OriginalProgramId], [ProgramName], [ProgramType], [Description], [ArchivedAtUtc])
            VALUES ({archivedProgram.Id}, {archivedProgram.OriginalProgramId}, {archivedProgram.ProgramName}, {archivedProgram.ProgramType}, {archivedProgram.Description}, {archivedProgram.ArchivedAtUtc});
            """);
    }

    private bool RequiresExplicitIdInsert(string tableName)
    {
        if (!_context.Database.IsSqlServer())
        {
            return false;
        }

        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT ISNULL(COLUMNPROPERTY(OBJECT_ID(N'{tableName}'), N'Id', 'IsIdentity'), -1)";
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
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT ISNULL(MAX([Id]), 0) + 1 FROM [{tableName}]";
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

    private void UnarchiveStrategyById(int id)
    {
        _context.Database.ExecuteSqlInterpolated($"""
            UPDATE [Strategies]
            SET [IsArchived] = 0,
                [ArchivedAtUtc] = NULL
            WHERE [Id] = {id} AND ISNULL([IsArchived], 0) = 1;
            """);
    }

    private List<ArchivedProgram> LoadArchivedProgramsForDisplay()
    {
        if (!_context.Database.IsSqlServer())
        {
            return _context.ArchivedPrograms
                .OrderByDescending(p => p.ArchivedAtUtc)
                .ThenByDescending(p => p.Id)
                .ToList();
        }

        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
        {
            connection.Open();
        }

        try
        {
            var existingColumns = GetSqlServerColumns(connection, "ArchivedPrograms");
            using var command = connection.CreateCommand();
            command.CommandText = BuildArchivedProgramsSelectSql(existingColumns);

            using var reader = command.ExecuteReader();
            var results = new List<ArchivedProgram>();
            while (reader.Read())
            {
                results.Add(new ArchivedProgram
                {
                    Id = SafeGetInt(reader, "Id"),
                    OriginalProgramId = SafeGetInt(reader, "OriginalProgramId"),
                    ProgramName = SafeGetString(reader, "ProgramName"),
                    ProgramType = SafeGetString(reader, "ProgramType"),
                    Description = SafeGetString(reader, "Description"),
                    ArchivedAtUtc = SafeGetNullableDateTime(reader, "ArchivedAtUtc") ?? DateTime.UtcNow
                });
            }

            return results;
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }

    private List<Strategy> LoadArchivedEventsForDisplay()
    {
        if (!_context.Database.IsSqlServer())
        {
            return _context.Strategies
                .Where(s => s.IsArchived)
                .OrderByDescending(s => s.ArchivedAtUtc ?? DateTime.MinValue)
                .ThenByDescending(s => s.Id)
                .ToList();
        }

        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
        {
            connection.Open();
        }

        try
        {
            var existingColumns = GetSqlServerColumns(connection, "Strategies");
            using var command = connection.CreateCommand();
            command.CommandText = BuildArchivedStrategiesSelectSql(existingColumns);

            using var reader = command.ExecuteReader();
            var results = new List<Strategy>();
            while (reader.Read())
            {
                results.Add(new Strategy
                {
                    Id = SafeGetInt(reader, "Id"),
                    Name = SafeGetString(reader, "Name"),
                    ProgramName = SafeGetNullableString(reader, "ProgramName"),
                    ProgramType = SafeGetNullableString(reader, "ProgramType"),
                    Description = SafeGetString(reader, "Description"),
                    IsArchived = SafeGetBool(reader, "IsArchived"),
                    ArchivedAtUtc = SafeGetNullableDateTime(reader, "ArchivedAtUtc")
                });
            }

            return results;
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }

    private static HashSet<string> GetSqlServerColumns(DbConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT [name]
            FROM sys.columns
            WHERE [object_id] = OBJECT_ID(@tableName);
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@tableName";
        parameter.Value = $"dbo.{tableName}";
        command.Parameters.Add(parameter);

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var name = reader["name"]?.ToString();
            if (!string.IsNullOrWhiteSpace(name))
            {
                columns.Add(name);
            }
        }

        return columns;
    }

    private static string BuildArchivedProgramsSelectSql(HashSet<string> existingColumns)
    {
        string SelectColumn(string name, string fallbackSql)
            => existingColumns.Contains(name) ? $"[{name}] AS [{name}]" : $"{fallbackSql} AS [{name}]";

        var selectList = string.Join(", ", new[]
        {
            SelectColumn("Id", "0"),
            SelectColumn("OriginalProgramId", "0"),
            SelectColumn("ProgramName", "N''"),
            SelectColumn("ProgramType", "N''"),
            SelectColumn("Description", "N''"),
            SelectColumn("ArchivedAtUtc", "CAST(NULL AS datetime2)")
        });

        var orderBy = existingColumns.Contains("ArchivedAtUtc")
            ? " ORDER BY [ArchivedAtUtc] DESC, [Id] DESC"
            : " ORDER BY [Id] DESC";

        return $"SELECT {selectList} FROM [dbo].[ArchivedPrograms]{orderBy};";
    }

    private static string BuildArchivedStrategiesSelectSql(HashSet<string> existingColumns)
    {
        string SelectColumn(string name, string fallbackSql)
            => existingColumns.Contains(name) ? $"[{name}] AS [{name}]" : $"{fallbackSql} AS [{name}]";

        var selectList = string.Join(", ", new[]
        {
            SelectColumn("Id", "0"),
            SelectColumn("Name", "N''"),
            SelectColumn("ProgramName", "NULL"),
            SelectColumn("ProgramType", "NULL"),
            SelectColumn("Description", "N''"),
            SelectColumn("IsArchived", "CAST(0 AS bit)"),
            SelectColumn("ArchivedAtUtc", "CAST(NULL AS datetime2)")
        });

        var whereSql = existingColumns.Contains("IsArchived")
            ? " WHERE ISNULL([IsArchived], 0) = 1"
            : " WHERE 1 = 0";

        var orderBy = existingColumns.Contains("ArchivedAtUtc")
            ? " ORDER BY [ArchivedAtUtc] DESC, [Id] DESC"
            : " ORDER BY [Id] DESC";

        return $"SELECT {selectList} FROM [dbo].[Strategies]{whereSql}{orderBy};";
    }

    private static string SafeGetString(DbDataReader reader, string name)
        => SafeGetNullableString(reader, name) ?? string.Empty;

    private static string? SafeGetNullableString(DbDataReader reader, string name)
    {
        var value = reader[name];
        return value == DBNull.Value ? null : value.ToString();
    }

    private static int SafeGetInt(DbDataReader reader, string name)
    {
        var value = reader[name];
        if (value == DBNull.Value)
        {
            return 0;
        }

        return value switch
        {
            int intValue => intValue,
            long longValue => Convert.ToInt32(longValue),
            short shortValue => shortValue,
            byte byteValue => byteValue,
            bool boolValue => boolValue ? 1 : 0,
            _ when int.TryParse(value.ToString(), out var parsed) => parsed,
            _ => 0
        };
    }

    private static bool SafeGetBool(DbDataReader reader, string name)
    {
        var value = reader[name];
        if (value == DBNull.Value)
        {
            return false;
        }

        return value switch
        {
            bool boolValue => boolValue,
            byte byteValue => byteValue != 0,
            short shortValue => shortValue != 0,
            int intValue => intValue != 0,
            long longValue => longValue != 0,
            string stringValue when bool.TryParse(stringValue, out var parsedBool) => parsedBool,
            string stringValue when int.TryParse(stringValue, out var parsedInt) => parsedInt != 0,
            _ => false
        };
    }

    private static DateTime? SafeGetNullableDateTime(DbDataReader reader, string name)
    {
        var value = reader[name];
        if (value == DBNull.Value)
        {
            return null;
        }

        return value switch
        {
            DateTime dateTimeValue => dateTimeValue,
            DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue.UtcDateTime,
            string stringValue when DateTime.TryParse(stringValue, out var parsedDate) => parsedDate,
            _ => null
        };
    }
}
