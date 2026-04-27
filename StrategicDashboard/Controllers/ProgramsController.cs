using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
using System.Security.Claims;
using System.Data;

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
                var linkedStrategies = _context.Strategies
                    .Where(s => s.ProgramId == program.Id)
                    .ToList();

                foreach (var strategy in linkedStrategies)
                {
                    strategy.ProgramId = null;
                }

                var archivedProgram = new ArchivedProgram
                {
                    OriginalProgramId = program.Id,
                    ProgramName = program.ProgramName,
                    ProgramType = program.ProgramType,
                    Description = program.Description,
                    ArchivedAtUtc = DateTime.UtcNow
                };

                _context.ArchivedPrograms.Add(archivedProgram);
                _context.Programs.Remove(program);
                _context.SaveChanges();

                _activityLog.Log(
                    GetActorName(),
                    "Archived Program",
                    "Program",
                    details: $"Id={program.Id}; Archived '{program.ProgramName}' ({program.ProgramType})");
                TempData["ProgramsSuccess"] = "Program archived successfully.";
            }
            catch (Exception)
            {
                TempData["ProgramsError"] = "We couldn't archive that program right now. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        return RedirectToAction(nameof(Archive));
    }

    [HttpGet]
    public IActionResult Archive()
    {
        var programs = _context.ArchivedPrograms
            .OrderByDescending(p => p.Id)
            .ToList();

        List<Strategy> events;
        try
        {
            events = _context.Strategies
                .Where(s => s.IsArchived)
                .OrderByDescending(s => s.ArchivedAtUtc ?? DateTime.MinValue)
                .ThenByDescending(s => s.Id)
                .ToList();
        }
        catch (Exception)
        {
            events = new List<Strategy>();
            TempData["ProgramsError"] ??= "Archived events couldn't be loaded right now, but archived programs are still available.";
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
        var archivedEvent = _context.Strategies.FirstOrDefault(s => s.Id == id && s.IsArchived);
        if (archivedEvent != null)
        {
            archivedEvent.IsArchived = false;
            archivedEvent.ArchivedAtUtc = null;
            _events.UnarchiveByStrategyTemplate(id);
            _context.SaveChanges();
            TempData["ProgramsSuccess"] = "Event restored.";
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
}
