using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace OneJaxDashboard.Controllers
{
    public class BoardMemberRecruitmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BoardMemberRecruitmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BoardMemberRecruitment/Index
        [HttpGet]
        public IActionResult Index()
        {
            var allEntries = _context.BoardMember_29D
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Quarter)
                .ToList();
            
            var totalRecruited = allEntries.Sum(e => e.NumberRecruited);
            var currentYearRecruited = allEntries.Where(e => e.Year == DateTime.Now.Year).Sum(e => e.NumberRecruited);
            
            ViewBag.TotalRecruited = totalRecruited;
            ViewBag.CurrentYearRecruited = currentYearRecruited;
            ViewBag.TotalEntries = allEntries.Count;
            ViewBag.AllEntries = allEntries;
            
            return View(new BoardMemberRecruitment());
        }

        // POST: BoardMemberRecruitment/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(BoardMemberRecruitment model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.BoardMember_29D.Add(model);
                    _context.SaveChanges();
                    
                    TempData["Success"] = "Board member recruitment record submitted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            var allEntries = _context.BoardMember_29D
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Quarter)
                .ToList();
            
            ViewBag.TotalRecruited = allEntries.Sum(e => e.NumberRecruited);
            ViewBag.CurrentYearRecruited = allEntries.Where(e => e.Year == DateTime.Now.Year).Sum(e => e.NumberRecruited);
            ViewBag.TotalEntries = allEntries.Count;
            ViewBag.AllEntries = allEntries;
            
            return View(model);
        }
    }
}
