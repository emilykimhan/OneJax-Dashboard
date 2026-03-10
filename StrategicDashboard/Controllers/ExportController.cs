using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;
using OneJaxDashboard.Data;
//karrie
public class ExportController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExportController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ─── Existing single-type export ─────────────────────────────────────────

    public IActionResult ExportStaffSurveyToExcel()
    {
        var surveys = _context.StaffSurveys_22D.ToList();

        using (var workbook = new ClosedXML.Excel.XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Staff Survey 22D");
            worksheet.Cell(1, 1).Value = "Year";
            worksheet.Cell(1, 2).Value = "SatisfactionRate";
            worksheet.Cell(1, 3).Value = "CreatedDate";

            for (int i = 0; i < surveys.Count; i++)
            {
                var s = surveys[i];
                worksheet.Cell(i + 2, 1).Value = s.Year;
                worksheet.Cell(i + 2, 2).Value = s.SatisfactionRate;
                worksheet.Cell(i + 2, 3).Value = s.CreatedDate.ToString("MM/dd/yyyy");
            }

            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "StaffSurveyData.xlsx");
            }
        }
    }

    // ─── Export Selected Records ─────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExportSelected(List<string> selectedItems, string format)
    {
        if (selectedItems == null || selectedItems.Count == 0)
        {
            TempData["Error"] = "No records selected for export.";
            return RedirectToAction("RecordHistory", "DataEntry");
        }

        // Group selected items by type
        var byType = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in selectedItems)
        {
            var parts = item.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out int id))
            {
                if (!byType.ContainsKey(parts[0]))
                    byType[parts[0]] = new List<int>();
                byType[parts[0]].Add(id);
            }
        }

        return BuildExcel(byType);
    }

    private IActionResult BuildExcel(Dictionary<string, List<int>> byType)
    {
        using var workbook = new XLWorkbook();

        void AddSheet(string name, string[] headers, IEnumerable<object?[]> rows)
        {
            var ws = workbook.Worksheets.Add(name.Length > 31 ? name[..31] : name);
            for (int c = 0; c < headers.Length; c++)
                ws.Cell(1, c + 1).Value = headers[c];
            int row = 2;
            foreach (var r in rows)
            {
                for (int c = 0; c < r.Length; c++)
                    ws.Cell(row, c + 1).Value = r[c]?.ToString() ?? "";
                row++;
            }
            ws.Row(1).Style.Font.Bold = true;
            ws.Columns().AdjustToContents();
        }

        if (byType.TryGetValue("staff-survey", out var ssIds))
        {
            var records = _context.StaffSurveys_22D.Where(x => ssIds.Contains(x.Id)).ToList();
            AddSheet("Staff Survey", new[] { "Year", "SatisfactionRate", "CreatedDate" },
                records.Select(x => new object?[] { x.Year, x.SatisfactionRate, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("professional-development", out var pdIds))
        {
            var records = _context.ProfessionalDevelopments.Where(x => pdIds.Contains(x.Id)).ToList();
            AddSheet("Professional Development", new[] { "Year", "StaffName", "Activities", "CreatedDate" },
                records.Select(x => new object?[] { x.Year, x.Name, x.Activities, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("media-placements", out var mpIds))
        {
            var records = _context.MediaPlacements_3D.Where(x => mpIds.Contains(x.Id)).ToList();
            AddSheet("Media Placements", new[] { "TotalMentions","Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec","CreatedDate" },
                records.Select(x => new object?[] { x.TotalMentions,x.January,x.February,x.March,x.April,x.May,x.June,x.July,x.August,x.September,x.October,x.November,x.December,x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("website-traffic", out var wtIds))
        {
            var records = _context.WebsiteTraffic.Where(x => wtIds.Contains(x.Id)).ToList();
            AddSheet("Website Traffic", new[] { "TotalClicks","Q1_JulSep","Q2_OctDec","Q3_JanMar","Q4_AprJun","CreatedDate" },
                records.Select(x => new object?[] { x.TotalClicks,x.Q1_JulySeptember,x.Q2_OctoberDecember,x.Q3_JanuaryMarch,x.Q4_AprilJune,x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("donor-events", out var deIds))
        {
            var records = _context.DonorEvents_19D.Include(d => d.Strategy).Where(x => deIds.Contains(x.Id)).ToList();
            AddSheet("Donor Engagement", new[] { "Event","NumberOfParticipants","EventSatisfactionRating","CreatedDate" },
                records.Select(x => new object?[] { x.Strategy?.Name ?? "", x.NumberOfParticipants, x.EventSatisfactionRating, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("comm-rate", out var crIds))
        {
            var records = _context.CommunicationRate.Where(x => crIds.Contains(x.Id)).ToList();
            AddSheet("Communication Satisfaction", new[] { "Year","AverageCommSatisfaction","CreatedDate" },
                records.Select(x => new object?[] { x.Year, x.AverageCommunicationSatisfaction, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("fee-for-service", out var ffIds))
        {
            var records = _context.FeeForServices_21D.Include(f => f.Strategy).Where(x => ffIds.Contains(x.Id)).ToList();
            AddSheet("Fee-For-Service", new[] { "ClientName","Event","WorkshopFormat","WorkshopLocation","WorkshopDate","Attendees","ParticipantSatisfaction","PartnerSatisfaction","Revenue","Expense","Year","CreatedDate" },
                records.Select(x => new object?[] { x.ClientName, x.Strategy?.Name ?? x.EventName ?? "", x.WorkshopFormat, x.WorkshopLocation ?? "", x.WorkshopDate.ToString("MM/dd/yyyy"), x.NumberOfAttendees, x.ParticipantSatisfactionRating, x.PartnerSatisfactionRating, x.RevenueReceived, x.ExpenseReceived, x.Year, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("earned-income", out var eiIds))
        {
            var records = _context.income_27D.Where(x => eiIds.Contains(x.Id)).ToList();
            AddSheet("Earned Income", new[] { "IncomeSource","Amount","Month","Notes","CreatedDate" },
                records.Select(x => new object?[] { x.IncomeSource, x.Amount, x.Month, x.Notes ?? "", x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("budget-tracking", out var btIds))
        {
            var records = _context.BudgetTracking_28D.Where(x => btIds.Contains(x.Id)).ToList();
            AddSheet("Budget Tracking", new[] { "Quarter","Year","TotalRevenues","TotalExpenses","NetAmount","Notes","CreatedDate" },
                records.Select(x => new object?[] { x.Quarter, x.Year, x.TotalRevenues, x.TotalExpenses, x.NetAmount, x.Notes ?? "", x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("social-media", out var smIds))
        {
            var records = _context.socialMedia_5D.Where(x => smIds.Contains(x.Id)).ToList();
            AddSheet("Social Media", new[] { "Year","AverageEngagementRate","JulSep","OctDec","JanMar","AprJun","GoalMet","CreatedDate" },
                records.Select(x => new object?[] { x.Year, x.AverageEngagementRate, x.JulySeptEngagementRate?.ToString() ?? "", x.OctDecEngagementRate?.ToString() ?? "", x.JanMarEngagementRate?.ToString() ?? "", x.AprilJuneEngagementRate?.ToString() ?? "", x.GoalMet, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("milestone", out var msIds))
        {
            var records = _context.achieveMile_6D.Where(x => msIds.Contains(x.Id)).ToList();
            AddSheet("Milestone Achievement", new[] { "MilestonesAchievedPercent","AchievedInReview","GoalMet","CreatedDate" },
                records.Select(x => new object?[] { x.Percentage, x.achievedReview, x.GoalMet, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("community-perception", out var cpIds))
        {
            var records = _context.Annual_average_7D.Where(x => cpIds.Contains(x.Id)).ToList();
            AddSheet("Community Perception", new[] { "Year","PercentTrustedLeader","TotalRespondents","RespondentsTrusted","Notes","GoalMet","CreatedDate" },
                records.Select(x => new object?[] { x.Year, x.Percentage, x.TotalRespondents?.ToString() ?? "", x.RespondentsIdentifyingAsTrusted?.ToString() ?? "", x.Notes ?? "", x.GoalMet, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("programs-demographics", out var demoIds))
        {
            var records = _context.demographics_8D.Include(d => d.Strategy).Where(x => demoIds.Contains(x.Id)).ToList();
            AddSheet("Programs Demographics", new[] { "Event","Year","ZipCodes","Notes","CreatedDate" },
                records.Select(x => new object?[] { x.Strategy?.Name ?? "", x.Year, x.ZipCodes, x.Notes ?? "", x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("framework-plan", out var fpIds))
        {
            var records = _context.Plan2026_24D.Where(x => fpIds.Contains(x.Id)).ToList();
            AddSheet("Framework Plan", new[] { "Name","Year","Quarter","FrameworkStatus","GoalMet","IssueName","CrisisDescription","IssueHandled","Notes","CreatedDate" },
                records.Select(x => new object?[] { x.Name, x.Year, x.Quarter, x.FrameworkStatus, x.GoalMet, x.IssueName ?? "", x.CrisisDescription ?? "", x.IssueHandled, x.Notes ?? "", x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("board-member", out var bmIds))
        {
            var records = _context.BoardMember_29D.Where(x => bmIds.Contains(x.Id)).ToList();
            AddSheet("Board Member Recruitment", new[] { "Year","Quarter","NumberRecruited","MemberNames","TotalProspectOutreach","ProspectNames","CreatedDate" },
                records.Select(x => new object?[] { x.Year, x.Quarter, x.NumberRecruited, x.MemberNames ?? "", x.TotalProspectOutreach, x.ProspectNames ?? "", x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("board-meeting", out var bmaIds))
        {
            var records = _context.BoardMeetingAttendance.Where(x => bmaIds.Contains(x.Id)).ToList();
            AddSheet("Board Meeting Attendance", new[] { "MeetingDate","MembersInAttendance","TotalBoardMembers","AttendanceRate","CreatedDate" },
                records.Select(x => new object?[] { x.MeetingDate.ToString("MM/dd/yyyy"), x.MembersInAttendance, x.TotalBoardMembers?.ToString() ?? "", x.AttendanceRate?.ToString("F1") ?? "", x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("self-assessment", out var saIds))
        {
            var records = _context.selfAssess_31D.Where(x => saIds.Contains(x.Id)).ToList();
            AddSheet("Board Self-Assessment", new[] { "Year","SelfAssessmentScore","CreatedDate" },
                records.Select(x => new object?[] { x.Year, x.SelfAssessmentScore, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("volunteer-program", out var vpIds))
        {
            var records = _context.volunteerProgram_40D.Where(x => vpIds.Contains(x.Id)).ToList();
            AddSheet("Volunteer Program", new[] { "Quarter","Year","NumberOfVolunteers","VolunteerLedInitiatives","CommunicationsActivities","RecognitionActivities","InitiativeDescriptions","CreatedDate" },
                records.Select(x => new object?[] { x.Quarter.ToString(), x.Year, x.NumberOfVolunteers, x.VolunteerLedInitiatives, x.CommunicationsActivities, x.RecognitionActivities, x.InitiativeDescriptions, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("interfaith-event", out var ieIds))
        {
            var records = _context.Interfaith_11D.Include(i => i.Strategy).Where(x => ieIds.Contains(x.Id)).ToList();
            AddSheet("Interfaith Events", new[] { "EventName","FaithsRepresented","PostEventSatisfaction","TotalAttendance","CreatedDate" },
                records.Select(x => new object?[] { x.Strategy?.Name ?? "", x.NumberOfFaithsRepresented, x.PostEventSatisfactionSurvey, x.TotalAttendance, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("event-satisfaction", out var esIds))
        {
            var records = _context.EventSatisfaction_12D.Include(e => e.Strategy).Where(x => esIds.Contains(x.Id)).ToList();
            AddSheet("Event Satisfaction", new[] { "EventName","AttendeeSatisfaction","CreatedDate" },
                records.Select(x => new object?[] { x.Strategy?.Name ?? "", x.EventAttendeeSatisfactionPercentage, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("faith-community", out var fcIds))
        {
            var records = _context.FaithCommunity_13D.Include(f => f.Strategy).Where(x => fcIds.Contains(x.Id)).ToList();
            AddSheet("Faith Community", new[] { "EventName","NumberOfFaithsRepresented","CreatedDate" },
                records.Select(x => new object?[] { x.Strategy?.Name ?? "", x.NumberOfFaithsRepresented, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("network-contacts", out var ncIds))
        {
            var records = _context.ContactsInterfaith_14D.Where(x => ncIds.Contains(x.Id)).ToList();
            AddSheet("Network Contacts", new[] { "Year","TotalInterfaithContacts","CreatedDate" },
                records.Select(x => new object?[] { x.Year, x.TotalInterfaithContacts, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("youth-attendance", out var yaIds))
        {
            var records = _context.YouthAttend_15D.Include(y => y.Strategy).Where(x => yaIds.Contains(x.Id)).ToList();
            AddSheet("Youth Attendance", new[] { "Event","NumberOfYouthAttendees","PostEventSurveySatisfaction","AveragePreAssessment","AveragePostAssessment","CreatedDate" },
                records.Select(x => new object?[] { x.Strategy?.Name ?? "", x.NumberOfYouthAttendees, x.PostEventSurveySatisfaction, x.AveragePreAssessment, x.AveragePostAssessment, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("participant-diversity", out var diversityIds))
        {
            var records = _context.Diversity_37D.Include(d => d.Strategy).Where(x => diversityIds.Contains(x.Id)).ToList();
            AddSheet("Participant Diversity", new[] { "FiscalYear","Event","DiversityCount","CreatedDate" },
                records.Select(x => new object?[] { x.FiscalYear, x.Strategy?.Name ?? "", x.DiversityCount, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("first-time-participants", out var ftpIds))
        {
            var records = _context.FirstTime_38D.Include(f => f.Strategy).Where(x => ftpIds.Contains(x.Id)).ToList();
            AddSheet("First-Time Participants", new[] { "FiscalYear","Event","TotalAttendees","FirstTimeParticipants","FirstTimeRate","GoalMet","CreatedDate" },
                records.Select(x => new object?[] { x.FiscalYear, x.Strategy?.Name ?? "", x.TotalAttendees, x.NumberOfFirstTimeParticipants, x.FirstTimeParticipantRate, x.GoalMet, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }
        if (byType.TryGetValue("collab-partners", out var collabIds))
        {
            var records = _context.CollabTouch_47D.Include(c => c.Strategy).Where(x => collabIds.Contains(x.Id)).ToList();
            AddSheet("Collab Partner Touchpoints", new[] { "FiscalYear","PartnerOrganization","Contact","ContactEmail","ContactPhone","Event","Touchpoint","CreatedDate" },
                records.Select(x => new object?[] { x.FiscalYear, x.PartnerOrganization, x.Contact, x.ContactEmail ?? "", x.ContactPhone ?? "", x.Strategy?.Name ?? "", x.Touchpoint, x.CreatedDate.ToString("MM/dd/yyyy") }));
        }

        if (!workbook.Worksheets.Any())
        {
            TempData["Error"] = "No matching records found for export.";
            return RedirectToAction("RecordHistory", "DataEntry");
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"SelectedRecords_{DateTime.Now:yyyyMMdd}.xlsx");
    }

}
