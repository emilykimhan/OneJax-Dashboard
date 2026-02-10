using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace OneJaxDashboard.Controllers
{
    public class BoardMeetingAttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BoardMeetingAttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BoardMeetingAttendance/Index
        [HttpGet]
        public IActionResult Index()
        {
            var allEntries = _context.BoardMeetingAttendance
                .OrderByDescending(b => b.MeetingDate)
                .ToList();

            var totalMeetings = allEntries.Count;
            var meetingsAtOrAbove90 = allEntries.Count(e =>
                e.TotalBoardMembers.HasValue && e.TotalBoardMembers.Value > 0 &&
                ((decimal)e.MembersInAttendance / e.TotalBoardMembers.Value * 100) >= 90);
            var overallAttendanceRate = totalMeetings > 0 && allEntries.Any(e => e.TotalBoardMembers.HasValue && e.TotalBoardMembers.Value > 0)
                ? Math.Round(allEntries
                    .Where(e => e.TotalBoardMembers.HasValue && e.TotalBoardMembers.Value > 0)
                    .Average(e => (decimal)e.MembersInAttendance / e.TotalBoardMembers!.Value * 100), 2)
                : 0;

            ViewBag.TotalMeetings = totalMeetings;
            ViewBag.MeetingsAtOrAbove90 = meetingsAtOrAbove90;
            ViewBag.OverallAttendanceRate = overallAttendanceRate;
            ViewBag.AllEntries = allEntries;

            return View(new BoardMeetingAttendance());
        }

        // POST: BoardMeetingAttendance/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(BoardMeetingAttendance model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.BoardMeetingAttendance.Add(model);
                    _context.SaveChanges();

                    TempData["Success"] = "Board meeting attendance record submitted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            var allEntries = _context.BoardMeetingAttendance
                .OrderByDescending(b => b.MeetingDate)
                .ToList();

            var totalMeetings = allEntries.Count;
            var meetingsAtOrAbove90 = allEntries.Count(e =>
                e.TotalBoardMembers.HasValue && e.TotalBoardMembers.Value > 0 &&
                ((decimal)e.MembersInAttendance / e.TotalBoardMembers.Value * 100) >= 90);
            var overallAttendanceRate = totalMeetings > 0 && allEntries.Any(e => e.TotalBoardMembers.HasValue && e.TotalBoardMembers.Value > 0)
                ? Math.Round(allEntries
                    .Where(e => e.TotalBoardMembers.HasValue && e.TotalBoardMembers.Value > 0)
                    .Average(e => (decimal)e.MembersInAttendance / e.TotalBoardMembers!.Value * 100), 2)
                : 0;

            ViewBag.TotalMeetings = totalMeetings;
            ViewBag.MeetingsAtOrAbove90 = meetingsAtOrAbove90;
            ViewBag.OverallAttendanceRate = overallAttendanceRate;
            ViewBag.AllEntries = allEntries;

            return View(model);
        }
    }
}
