using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Common;
//dina
[Authorize(Roles = "Admin,Staff")]
public class StrategyController : Controller
{
    private const string DashboardSyncOwnerUsername = "staff";
    private readonly ApplicationDbContext _context;
    private readonly ActivityLogService _activityLog;
    private readonly EventsService _events;
    private static readonly DateTime MaxEventDate = new(2030, 12, 31);
    private static readonly string[] DefaultProgramTypes =
    {
        "Humanitarian Awards",
        "Fundraising",
        "Youth",
        "Interfaith",
        "Community",
        "Donor"
    };

    public StrategyController(ApplicationDbContext context, ActivityLogService activityLog, EventsService events)
    {
        _context = context;
        _activityLog = activityLog;
        _events = events;
    }

    private static readonly List<SelectListItem> Goals = new()
    {
        new SelectListItem { Value = "1", Text = "Organizational Building" },
        new SelectListItem { Value = "2", Text = "Financial Sustainability" },
        new SelectListItem { Value = "3", Text = "Identity/Value Proposition" },
        new SelectListItem { Value = "4", Text = "Community Engagement" }
    };

    private List<SelectListItem> GetGoalOptions()
    {
        try
        {
            var dbGoals = _context.StrategicGoals
                .Where(g => g.Id >= 1 && g.Id <= 4)
                .OrderBy(g => g.Id)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
                .ToList();

            return dbGoals.Count > 0 ? dbGoals : Goals;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[strategy-index] Failed to load strategic goals: {ex}");
            return Goals
                .Select(goal => new SelectListItem
                {
                    Value = goal.Value,
                    Text = goal.Text
                })
                .ToList();
        }
    }

    private StrategicGoal? EnsureGoalExists(int goalId)
    {
        var fallbackGoalName = Goals.FirstOrDefault(g => g.Value == goalId.ToString())?.Text;
        if (string.IsNullOrWhiteSpace(fallbackGoalName))
        {
            return null;
        }

        var existingGoal = _context.StrategicGoals.FirstOrDefault(g => g.Id == goalId);
        if (existingGoal != null)
        {
            return existingGoal;
        }

        var newGoal = new StrategicGoal
        {
            Id = goalId,
            Name = fallbackGoalName
        };

        _context.StrategicGoals.Add(newGoal);
        return newGoal;
    }

    private static string ComputeFiscalYear(string? eventDate)
    {
        if (string.IsNullOrWhiteSpace(eventDate) || !DateTime.TryParse(eventDate, out var parsedDate))
        {
            return string.Empty;
        }

        var startYear = parsedDate.Month >= 7 ? parsedDate.Year : parsedDate.Year - 1;
        var endYear = startYear + 1;
        return $"{startYear}/{endYear}";
    }

    private static bool IsPastMaxEventDate(string? eventDate)
    {
        return DateTime.TryParse(eventDate, out var parsedDate) && parsedDate.Date > MaxEventDate;
    }

    private IActionResult RenderIndex(int? goalId, Dictionary<string, string>? formValues = null, Dictionary<string, string>? formErrors = null)
    {
        var pageErrors = new List<string>();
        List<Programs> programOptions;
        try
        {
            programOptions = _context.Programs
                .OrderBy(p => p.ProgramName)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[strategy-index] Failed to load programs: {ex}");
            programOptions = new List<Programs>();
            pageErrors.Add("Programs couldn't be loaded right now.");
        }

        var goalOptions = GetGoalOptions();
        ViewBag.Goals = goalOptions;
        ViewBag.Programs = programOptions;
        ViewBag.ProgramTypes = DefaultProgramTypes
            .Concat(programOptions
                .Select(p => p.ProgramType)
                .Where(t => !string.IsNullOrWhiteSpace(t)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        List<Strategy> goalStrategies;
        try
        {
            goalStrategies = LoadStrategiesForDisplay(goalId, includeArchived: false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[strategy-index] Failed to load strategies: {ex}");
            goalStrategies = new List<Strategy>();
            pageErrors.Add("Events couldn't be loaded right now.");
        }

        goalStrategies = goalStrategies.OrderByDescending(s => s.Id).ToList();

        ViewBag.GoalId = goalId;
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.PageErrorMessage = pageErrors.Count > 0 ? string.Join(" ", pageErrors) : null;
        ViewBag.FormValues = formValues ?? new Dictionary<string, string>();
        ViewBag.FormErrors = formErrors ?? new Dictionary<string, string>();

        return View("Index", goalStrategies);
    }

    private static Dictionary<string, string> BuildStrategyFormValues(
        int goalId,
        string? eventName,
        string? eventDescription,
        string? eventDate,
        string? eventTime,
        bool isCrossCollaboration,
        string? partners,
        int? programId,
        string? programType)
    {
        return new Dictionary<string, string>
        {
            ["goalId"] = goalId > 0 ? goalId.ToString() : string.Empty,
            ["eventName"] = eventName?.Trim() ?? string.Empty,
            ["eventDescription"] = eventDescription?.Trim() ?? string.Empty,
            ["eventDate"] = eventDate?.Trim() ?? string.Empty,
            ["eventTime"] = eventTime?.Trim() ?? string.Empty,
            ["isCrossCollaboration"] = isCrossCollaboration ? "true" : string.Empty,
            ["partners"] = partners?.Trim() ?? string.Empty,
            ["programId"] = programId?.ToString() ?? string.Empty,
            ["programType"] = programType?.Trim() ?? string.Empty
        };
    }

    public IActionResult Index(int? goalId)
    {
        return RenderIndex(goalId);
    }

    [HttpPost]
    public IActionResult Add(int goalId, string? eventName, string eventDescription, string? eventDate, string? eventTime, bool isCrossCollaboration = false, string? partners = null, int? programId = null, string? programType = null)
    {
        var normalizedDescription = eventDescription?.Trim() ?? string.Empty;
        var formValues = BuildStrategyFormValues(goalId, eventName, eventDescription, eventDate, eventTime, isCrossCollaboration, partners, programId, programType);
        var formErrors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(eventName))
        {
            formErrors["eventName"] = "Event name is required.";
        }

        if (goalId <= 0)
        {
            formErrors["goalId"] = "Assign to a goal is required.";
        }

        if (string.IsNullOrWhiteSpace(eventDate))
        {
            formErrors["eventDate"] = "Date is required.";
        }

        if (IsPastMaxEventDate(eventDate))
        {
            formErrors["eventDate"] = "Event date cannot be later than 12/31/2030.";
        }

        if (formErrors.Count > 0)
        {
            return RenderIndex(null, formValues, formErrors);
        }

        var selectedGoal = EnsureGoalExists(goalId);
        if (selectedGoal == null)
        {
            formErrors["goalId"] = "Please select a valid goal.";
            return RenderIndex(null, formValues, formErrors);
        }

        var selectedProgram = programId.HasValue
            ? _context.Programs.FirstOrDefault(p => p.Id == programId.Value)
            : null;

        var selectedProgramType = selectedProgram?.ProgramType;
        if (string.IsNullOrWhiteSpace(selectedProgramType) && !string.IsNullOrWhiteSpace(programType))
        {
            selectedProgramType = programType.Trim();
        }

        var resolvedEventName = eventName!.Trim();

        // Save to database for persistence - only set properties that don't have foreign key constraints
        var dbEvent = new Strategy
        {
            Name = resolvedEventName,
            ProgramId = selectedProgram?.Id,
            ProgramName = selectedProgram?.ProgramName,
            ProgramType = selectedProgramType,
            Description = normalizedDescription,
            StrategicGoalId = goalId,
            Date = eventDate,
            Time = eventTime,
            CrossCollaboration = isCrossCollaboration ? "Yes" : "No",
            Partners = isCrossCollaboration ? (partners ?? string.Empty).Trim() : string.Empty,
            EventFYear = ComputeFiscalYear(eventDate)
        };

        try
        {
            PersistStrategy(dbEvent);
            TrySyncLinkedDashboardEvent(dbEvent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[strategy-add] Failed to create strategy event '{resolvedEventName}': {ex}");
            formErrors["general"] = "We couldn't save that event right now. Please try again.";
            return RenderIndex(null, formValues, formErrors);
        }


        string goalName = selectedGoal.Name;
        // Log the creation
        _activityLog.Log(GetActorName(), "Created Core Strategy Event", "Strategy",
            details: $"Id={dbEvent.Id}; Created strategy event '{eventName}' under {goalName}");
        TempData["SuccessMessage"] = $"Successfully added event under “{goalName}”";

        return RedirectToAction(nameof(Index), new { goalId });
    }
    // POST: /Strategy/Edit

    [HttpGet]
    public IActionResult Edit(int id)
    {
        // Fetch the strategy from the database
        var evt = GetStrategyForMutation(id);
        if (evt == null)
        {
            return NotFound(); // Return 404 if the strategy doesn't exist
        }

        ViewBag.Goals = GetGoalOptions(); // Pass goals for the dropdown
        var programOptions = _context.Programs
            .OrderBy(p => p.ProgramName)
            .ToList();
        ViewBag.Programs = programOptions;
        ViewBag.ProgramTypes = DefaultProgramTypes
            .Concat(programOptions
                .Select(p => p.ProgramType)
                .Where(t => !string.IsNullOrWhiteSpace(t)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
        return View(evt); // Pass the strategy to the view
    }

    [HttpPost]
    public IActionResult Edit(int id, string? eventName, string eventDescription, string? eventDate, string? eventTime, int goalId, bool isCrossCollaboration = false, string? partners = null, int? programId = null, string? programType = null)
    {
        var normalizedDescription = eventDescription?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(eventName))
        {
            TempData["ErrorMessage"] = "Event name is required.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        if (goalId <= 0)
        {
            TempData["ErrorMessage"] = "Assign to Goal is required.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        if (string.IsNullOrWhiteSpace(eventDate))
        {
            TempData["ErrorMessage"] = "Event date is required.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        if (IsPastMaxEventDate(eventDate))
        {
            TempData["ErrorMessage"] = "Event date cannot be later than 12/31/2030.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        if (EnsureGoalExists(goalId) == null)
        {
            TempData["ErrorMessage"] = "Please select a valid goal before updating the event.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // Fetch the strategy from the database
        var evt = GetStrategyForMutation(id);
        if (evt == null)
        {
            return NotFound(); // Return 404 if the strategy doesn't exist
        }

        var selectedProgram = programId.HasValue
            ? _context.Programs.FirstOrDefault(p => p.Id == programId.Value)
            : null;

        var selectedProgramType = selectedProgram?.ProgramType;
        if (string.IsNullOrWhiteSpace(selectedProgramType) && !string.IsNullOrWhiteSpace(programType))
        {
            selectedProgramType = programType.Trim();
        }

        var resolvedEventName = eventName.Trim();

        var previousName = evt.Name;
        var previousProgramName = evt.ProgramName;
        var previousProgramType = evt.ProgramType;
        var previousCrossCollaboration = evt.CrossCollaboration;
        var previousPartners = evt.Partners;
        var previousDescription = evt.Description;
        var previousDate = evt.Date;
        var previousTime = evt.Time;
        var previousGoalId = evt.StrategicGoalId;
        var previousEventFYear = evt.EventFYear;

        // Update the strategy's properties
        evt.Name = resolvedEventName;
        evt.ProgramId = selectedProgram?.Id;
        evt.ProgramName = selectedProgram?.ProgramName;
        evt.ProgramType = selectedProgramType;
        evt.CrossCollaboration = isCrossCollaboration ? "Yes" : "No";
        evt.Partners = isCrossCollaboration ? (partners ?? string.Empty).Trim() : string.Empty;
        evt.Description = normalizedDescription;
        evt.Date = eventDate;
        evt.Time = eventTime;
        evt.StrategicGoalId = goalId;
        evt.EventFYear = ComputeFiscalYear(eventDate);

        try
        {
            SaveStrategyChanges(evt);
            TrySyncLinkedDashboardEvent(evt);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[strategy-edit] Failed to update strategy event '{resolvedEventName}' (Id={id}): {ex}");
            TempData["ErrorMessage"] = "We couldn't update that event right now. Please try again.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var previousGoalName = ResolveGoalName(previousGoalId);
        var updatedGoalName = ResolveGoalName(evt.StrategicGoalId);

        var changes = new List<string>();
        AddChange(changes, "Event Name", previousName, evt.Name);
        AddChange(changes, "Program Name", previousProgramName, evt.ProgramName);
        AddChange(changes, "Program Type", previousProgramType, evt.ProgramType);
        AddChange(changes, "Cross Collaboration", previousCrossCollaboration, evt.CrossCollaboration);
        AddChange(changes, "Partners", previousPartners, evt.Partners);
        AddChange(changes, "Description", previousDescription, evt.Description);
        AddChange(changes, "Date", previousDate, evt.Date);
        AddChange(changes, "Time", previousTime, evt.Time);
        AddChange(changes, "Strategic Goal", previousGoalName, updatedGoalName);
        AddChange(changes, "Fiscal Year", previousEventFYear, evt.EventFYear);
        var changeDetails = changes.Count > 0 ? string.Join("; ", changes) : "No field changes detected";

        _activityLog.Log(GetActorName(), "Updated Core Strategy Event", "Strategy",
            details: $"Id={evt.Id}; Updated '{evt.Name}'. Changes: {changeDetails}");

        TempData["SuccessMessage"] = "Event updated successfully!";
        return RedirectToAction(nameof(ViewEvents));
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var deletedEventName = GetStrategyNameForMutation(id);
        if (deletedEventName == null)
        {
            return NotFound();
        }

        try
        {
            using var transaction = _context.Database.BeginTransaction();
            DeleteEventsByStrategyTemplate(id);
            DeleteStrategyById(id);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[strategy-delete] Failed to delete strategy event '{deletedEventName}' (Id={id}): {ex}");
            TempData["ErrorMessage"] = "We couldn't delete that event right now. Please try again.";
            return RedirectToAction(nameof(ViewEvents));
        }

        _activityLog.Log(GetActorName(), "Deleted Core Strategy Event", "Strategy",
            details: $"Id={id}; Deleted '{deletedEventName}'");

        TempData["SuccessMessage"] = "Event deleted successfully!";
        return RedirectToAction(nameof(ViewEvents));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Archive(int id)
    {
        var archivedEventName = GetStrategyNameForMutation(id);
        if (archivedEventName == null)
        {
            return NotFound();
        }

        try
        {
            using var transaction = _context.Database.BeginTransaction();
            var archivedAtUtc = DateTime.UtcNow;
            ArchiveEventsByStrategyTemplate(id, archivedAtUtc);
            ArchiveStrategyById(id, archivedAtUtc);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[strategy-archive] Failed to archive strategy event '{archivedEventName}' (Id={id}): {ex}");
            TempData["ErrorMessage"] = "We couldn't archive that event right now. Please try again.";
            return RedirectToAction(nameof(ViewEvents));
        }

        TempData["ProgramsSuccess"] = "Event archived.";
        return RedirectToAction("Archive", "Programs");
    }

    public IActionResult ViewEvents(string? fy = null) 
    {
        var pageErrors = new List<string>();
        List<Strategy> events;
        try
        {
            events = LoadStrategiesForDisplay(goalId: null, includeArchived: false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[strategy-view-events] Failed to load strategies: {ex}");
            events = new List<Strategy>();
            pageErrors.Add("Events couldn't be loaded right now.");
        }

        var hasUpdates = false;
        foreach (var evt in events)
        {
            var computed = ComputeFiscalYear(evt.Date);
            if (!string.Equals(evt.EventFYear ?? string.Empty, computed, StringComparison.Ordinal))
            {
                evt.EventFYear = computed;
                hasUpdates = true;
            }
        }

        if (hasUpdates)
        {
            _context.SaveChanges();
        }

        // ViewEvents should only show the four strategic goals as goal filter tabs.
        try
        {
            ViewBag.Goals = _context.StrategicGoals
                .Where(g => g.Id >= 1 && g.Id <= 4)
                .OrderBy(g => g.Id)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[strategy-view-events] Failed to load strategic goals: {ex}");
            ViewBag.Goals = Goals
                .Select(goal => new SelectListItem
                {
                    Value = goal.Value,
                    Text = goal.Text
                })
                .ToList();
            pageErrors.Add("Goals couldn't be loaded right now.");
        }

        var fiscalYears = events
            .Select(e => FiscalYearSelection.ToEventsFormat(e.EventFYear))
            .Where(fy => !string.IsNullOrWhiteSpace(fy))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(fy => fy)
            .ToList();

        if (!fiscalYears.Any())
        {
            var now = DateTime.Now;
            var currentStartYear = now.Month >= 7 ? now.Year : now.Year - 1;
            fiscalYears = new List<string>
            {
                $"{currentStartYear - 1}/{currentStartYear}",
                $"{currentStartYear}/{currentStartYear + 1}",
                $"{currentStartYear + 1}/{currentStartYear + 2}"
            };
        }

        var selectedFiscalYear = FiscalYearSelection.ResolveEventsFiscalYear(Request, fy);
        if (!string.IsNullOrWhiteSpace(selectedFiscalYear)
            && !fiscalYears.Contains(selectedFiscalYear, StringComparer.OrdinalIgnoreCase))
        {
            fiscalYears.Add(selectedFiscalYear);
            fiscalYears = fiscalYears
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value)
                .ToList();
        }

        ViewBag.FiscalYears = fiscalYears;
        ViewBag.SelectedFY = selectedFiscalYear;
        ViewBag.PageErrorMessage = pageErrors.Count > 0 ? string.Join(" ", pageErrors) : null;
        FiscalYearSelection.PersistSelection(Response, selectedFiscalYear);

        // Pass the events to the view
        return View(events);
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

    private void TrySyncLinkedDashboardEvent(Strategy strategy)
    {
        try
        {
            SyncLinkedDashboardEvent(strategy);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[strategy-sync] Failed to sync dashboard event for strategy {strategy.Id}: {ex}");
        }
    }

    private void SyncLinkedDashboardEvent(Strategy strategy)
    {
        var canonicalEvent = _context.Events
            .FirstOrDefault(e => e.StrategyId == strategy.Id && !e.IsAssignedByAdmin);

        var resolvedOwnerUsername = ResolveDashboardSyncOwnerUsername();
        if (canonicalEvent == null)
        {
            if (string.IsNullOrWhiteSpace(resolvedOwnerUsername))
            {
                return;
            }

            canonicalEvent = new Event
            {
                StrategyId = strategy.Id,
                OwnerUsername = resolvedOwnerUsername,
                Status = "Planned",
                IsAssignedByAdmin = false
            };

            _context.Events.Add(canonicalEvent);
        }
        else if (string.IsNullOrWhiteSpace(canonicalEvent.OwnerUsername) && !string.IsNullOrWhiteSpace(resolvedOwnerUsername))
        {
            canonicalEvent.OwnerUsername = resolvedOwnerUsername;
        }

        canonicalEvent.Title = strategy.Name;
        canonicalEvent.Description = strategy.Description;
        canonicalEvent.StrategyId = strategy.Id;
        canonicalEvent.Type = strategy.ProgramType ?? canonicalEvent.Type ?? string.Empty;
        canonicalEvent.DueDate = ParseStrategyDate(strategy.Date);
    }

    private string? ResolveDashboardSyncOwnerUsername()
    {
        try
        {
            if (_context.Staffauth.Any(s => s.Username == DashboardSyncOwnerUsername))
            {
                return DashboardSyncOwnerUsername;
            }

            var currentUsername = User.Identity?.Name?.Trim();
            if (!string.IsNullOrWhiteSpace(currentUsername)
                && _context.Staffauth.Any(s => s.Username == currentUsername))
            {
                return currentUsername;
            }

            return _context.Staffauth
                .OrderByDescending(s => s.IsAdmin)
                .ThenBy(s => s.Username)
                .Select(s => s.Username)
                .FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[strategy-sync] Failed to resolve dashboard sync owner: {ex}");
            return null;
        }
    }

    private static DateTime? ParseStrategyDate(string? eventDate)
    {
        if (string.IsNullOrWhiteSpace(eventDate))
        {
            return null;
        }

        return DateTime.TryParse(eventDate, out var parsedDate)
            ? parsedDate
            : null;
    }

    private string ResolveGoalName(int? goalId)
    {
        if (!goalId.HasValue)
        {
            return "(empty)";
        }

        var goalName = _context.StrategicGoals
            .Where(g => g.Id == goalId.Value)
            .Select(g => g.Name)
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(goalName))
        {
            return goalName;
        }

        var fallback = Goals.FirstOrDefault(g => g.Value == goalId.Value.ToString())?.Text;
        return string.IsNullOrWhiteSpace(fallback) ? $"Goal {goalId.Value}" : fallback;
    }

    private void PersistStrategy(Strategy strategy)
    {
        if (!RequiresExplicitIdInsert("Strategies"))
        {
            _context.Strategies.Add(strategy);
            _context.SaveChanges();
            return;
        }

        strategy.Id = GetNextSqlServerId("Strategies");

        _context.Database.ExecuteSqlInterpolated($"""
            INSERT INTO [Strategies]
                ([Id], [Name], [ProgramId], [ProgramName], [ProgramType], [StrategicGoalId], [Description], [Date], [Time], [CrossCollaboration], [Partners], [EventFYear], [IsArchived], [ArchivedAtUtc])
            VALUES
                ({strategy.Id}, {strategy.Name}, {strategy.ProgramId}, {strategy.ProgramName}, {strategy.ProgramType}, {strategy.StrategicGoalId}, {strategy.Description}, {strategy.Date}, {strategy.Time}, {strategy.CrossCollaboration}, {strategy.Partners}, {strategy.EventFYear}, {strategy.IsArchived}, {strategy.ArchivedAtUtc});
            """);
    }

    private Strategy? GetStrategyForMutation(int id)
    {
        if (!_context.Database.IsSqlServer())
        {
            return _context.Strategies.FirstOrDefault(s => s.Id == id);
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
            command.CommandText = BuildStrategySelectByIdSql(existingColumns);

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.Value = id;
            command.Parameters.Add(parameter);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return new Strategy
            {
                Id = SafeGetInt(reader, "Id"),
                Name = SafeGetString(reader, "Name"),
                ProgramId = SafeGetNullableInt(reader, "ProgramId"),
                ProgramName = SafeGetNullableString(reader, "ProgramName"),
                ProgramType = SafeGetNullableString(reader, "ProgramType"),
                StrategicGoalId = SafeGetInt(reader, "StrategicGoalId"),
                Description = SafeGetString(reader, "Description"),
                Date = SafeGetNullableString(reader, "Date"),
                Time = SafeGetNullableString(reader, "Time"),
                CrossCollaboration = SafeGetString(reader, "CrossCollaboration"),
                Partners = SafeGetString(reader, "Partners"),
                EventFYear = SafeGetString(reader, "EventFYear"),
                IsArchived = SafeGetBool(reader, "IsArchived"),
                ArchivedAtUtc = SafeGetNullableDateTime(reader, "ArchivedAtUtc")
            };
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }

    private void SaveStrategyChanges(Strategy strategy)
    {
        if (!_context.Database.IsSqlServer())
        {
            _context.SaveChanges();
            return;
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
            command.CommandText = BuildStrategyUpdateSql(existingColumns);

            AddCommandParameter(command, "@id", strategy.Id);
            AddCommandParameter(command, "@name", strategy.Name);
            AddCommandParameter(command, "@programId", strategy.ProgramId);
            AddCommandParameter(command, "@programName", strategy.ProgramName);
            AddCommandParameter(command, "@programType", strategy.ProgramType);
            AddCommandParameter(command, "@strategicGoalId", strategy.StrategicGoalId);
            AddCommandParameter(command, "@description", strategy.Description);
            AddCommandParameter(command, "@date", strategy.Date);
            AddCommandParameter(command, "@time", strategy.Time);
            AddCommandParameter(command, "@crossCollaboration", strategy.CrossCollaboration);
            AddCommandParameter(command, "@partners", strategy.Partners);
            AddCommandParameter(command, "@eventFYear", strategy.EventFYear);

            var rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected == 0)
            {
                throw new InvalidOperationException($"Strategy event Id={strategy.Id} was not found.");
            }
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
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

    private string? GetStrategyNameForMutation(int id)
    {
        if (!_context.Database.IsSqlServer())
        {
            return _context.Strategies
                .Where(s => s.Id == id)
                .Select(s => s.Name)
                .FirstOrDefault();
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
            command.CommandText = """
                SELECT TOP (1) [Name]
                FROM [dbo].[Strategies]
                WHERE [Id] = @id;
                """;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.Value = id;
            command.Parameters.Add(parameter);

            var result = command.ExecuteScalar();
            return result == null || result == DBNull.Value ? null : result.ToString();
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }

    private void DeleteEventsByStrategyTemplate(int strategyId)
    {
        _context.Database.ExecuteSqlInterpolated($"""
            DELETE FROM [Events]
            WHERE [StrategyId] = {strategyId};
            """);
    }

    private void DeleteStrategyById(int strategyId)
    {
        _context.Database.ExecuteSqlInterpolated($"""
            DELETE FROM [Strategies]
            WHERE [Id] = {strategyId};
            """);
    }

    private void ArchiveEventsByStrategyTemplate(int strategyId, DateTime completionDateUtc)
    {
        _context.Database.ExecuteSqlInterpolated($"""
            UPDATE [Events]
            SET [IsArchived] = 1,
                [CompletionDate] = COALESCE([CompletionDate], {completionDateUtc})
            WHERE [StrategyId] = {strategyId};
            """);
    }

    private void ArchiveStrategyById(int strategyId, DateTime archivedAtUtc)
    {
        _context.Database.ExecuteSqlInterpolated($"""
            UPDATE [Strategies]
            SET [IsArchived] = 1,
                [ArchivedAtUtc] = {archivedAtUtc}
            WHERE [Id] = {strategyId};
            """);
    }

    private List<Strategy> LoadStrategiesForDisplay(int? goalId, bool includeArchived)
    {
        if (!_context.Database.IsSqlServer())
        {
            var query = _context.Strategies.AsQueryable();
            if (!includeArchived)
            {
                query = query.Where(s => !s.IsArchived);
            }

            if (goalId.HasValue)
            {
                query = query.Where(s => s.StrategicGoalId == goalId.Value);
            }

            return query.ToList();
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
            command.CommandText = BuildStrategiesSelectSql(existingColumns, goalId, includeArchived);

            if (goalId.HasValue && existingColumns.Contains("StrategicGoalId"))
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@goalId";
                parameter.Value = goalId.Value;
                command.Parameters.Add(parameter);
            }

            using var reader = command.ExecuteReader();
            var results = new List<Strategy>();
            while (reader.Read())
            {
                results.Add(new Strategy
                {
                    Id = SafeGetInt(reader, "Id"),
                    Name = SafeGetString(reader, "Name"),
                    ProgramId = SafeGetNullableInt(reader, "ProgramId"),
                    ProgramName = SafeGetNullableString(reader, "ProgramName"),
                    ProgramType = SafeGetNullableString(reader, "ProgramType"),
                    StrategicGoalId = SafeGetInt(reader, "StrategicGoalId"),
                    Description = SafeGetString(reader, "Description"),
                    Date = SafeGetNullableString(reader, "Date"),
                    Time = SafeGetNullableString(reader, "Time"),
                    CrossCollaboration = SafeGetString(reader, "CrossCollaboration"),
                    Partners = SafeGetString(reader, "Partners"),
                    EventFYear = SafeGetString(reader, "EventFYear"),
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

    private static void AddCommandParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static string BuildStrategiesSelectSql(HashSet<string> existingColumns, int? goalId, bool includeArchived)
    {
        string SelectColumn(string name, string fallbackSql)
            => existingColumns.Contains(name) ? $"[{name}] AS [{name}]" : $"{fallbackSql} AS [{name}]";

        var selectList = string.Join(", ", new[]
        {
            SelectColumn("Id", "0"),
            SelectColumn("Name", "N''"),
            SelectColumn("ProgramId", "NULL"),
            SelectColumn("ProgramName", "NULL"),
            SelectColumn("ProgramType", "NULL"),
            SelectColumn("StrategicGoalId", "0"),
            SelectColumn("Description", "N''"),
            SelectColumn("Date", "NULL"),
            SelectColumn("Time", "NULL"),
            SelectColumn("CrossCollaboration", "N''"),
            SelectColumn("Partners", "N''"),
            SelectColumn("EventFYear", "N''"),
            SelectColumn("IsArchived", "CAST(0 AS bit)"),
            SelectColumn("ArchivedAtUtc", "CAST(NULL AS datetime2)")
        });

        var whereClauses = new List<string>();
        if (!includeArchived && existingColumns.Contains("IsArchived"))
        {
            whereClauses.Add("ISNULL([IsArchived], 0) = 0");
        }

        if (goalId.HasValue && existingColumns.Contains("StrategicGoalId"))
        {
            whereClauses.Add("[StrategicGoalId] = @goalId");
        }

        var whereSql = whereClauses.Count > 0
            ? $" WHERE {string.Join(" AND ", whereClauses)}"
            : string.Empty;

        return $"SELECT {selectList} FROM [dbo].[Strategies]{whereSql};";
    }

    private static string BuildStrategySelectByIdSql(HashSet<string> existingColumns)
    {
        string SelectColumn(string name, string fallbackSql)
            => existingColumns.Contains(name) ? $"[{name}] AS [{name}]" : $"{fallbackSql} AS [{name}]";

        var selectList = string.Join(", ", new[]
        {
            SelectColumn("Id", "0"),
            SelectColumn("Name", "N''"),
            SelectColumn("ProgramId", "NULL"),
            SelectColumn("ProgramName", "NULL"),
            SelectColumn("ProgramType", "NULL"),
            SelectColumn("StrategicGoalId", "0"),
            SelectColumn("Description", "N''"),
            SelectColumn("Date", "NULL"),
            SelectColumn("Time", "NULL"),
            SelectColumn("CrossCollaboration", "N''"),
            SelectColumn("Partners", "N''"),
            SelectColumn("EventFYear", "N''"),
            SelectColumn("IsArchived", "CAST(0 AS bit)"),
            SelectColumn("ArchivedAtUtc", "CAST(NULL AS datetime2)")
        });

        return $"SELECT TOP (1) {selectList} FROM [dbo].[Strategies] WHERE [Id] = @id;";
    }

    private static string BuildStrategyUpdateSql(HashSet<string> existingColumns)
    {
        var updates = new List<(string Column, string Parameter)>
        {
            ("Name", "@name"),
            ("ProgramId", "@programId"),
            ("ProgramName", "@programName"),
            ("ProgramType", "@programType"),
            ("StrategicGoalId", "@strategicGoalId"),
            ("Description", "@description"),
            ("Date", "@date"),
            ("Time", "@time"),
            ("CrossCollaboration", "@crossCollaboration"),
            ("Partners", "@partners"),
            ("EventFYear", "@eventFYear")
        }
        .Where(update => existingColumns.Contains(update.Column))
        .ToList();

        if (updates.Count == 0)
        {
            throw new InvalidOperationException("The Strategies table has no editable event columns.");
        }

        var setSql = string.Join(", ", updates.Select(update => $"[{update.Column}] = {update.Parameter}"));
        return $"UPDATE [dbo].[Strategies] SET {setSql} WHERE [Id] = @id;";
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

    private static int? SafeGetNullableInt(DbDataReader reader, string name)
    {
        var value = reader[name];
        if (value == DBNull.Value)
        {
            return null;
        }

        return value switch
        {
            int intValue => intValue,
            long longValue => Convert.ToInt32(longValue),
            short shortValue => shortValue,
            byte byteValue => byteValue,
            _ when int.TryParse(value.ToString(), out var parsed) => parsed,
            _ => null
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
