using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Controllers;

public class ProgramsController : Controller
{
    private readonly ApplicationDbContext _context;

    private static readonly string[] ProgramTypes =
    {
        "Humanitarian Awards",
        "Fundraising",
        "Youth",
        "Interfaith",
        "Community"
    };

    public ProgramsController(ApplicationDbContext context)
    {
        _context = context;
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

        _context.Programs.Add(program);
        _context.SaveChanges();

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

        program.ProgramName = programName.Trim();
        program.ProgramType = programType.Trim();
        program.Description = (description ?? string.Empty).Trim();

        _context.SaveChanges();

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
            using var transaction = _context.Database.BeginTransaction();

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

            transaction.Commit();
            TempData["ProgramsSuccess"] = "Program archived.";
        }

        return RedirectToAction(nameof(Archive));
    }

    [HttpGet]
    public IActionResult Archive()
    {
        var programs = _context.ArchivedPrograms
            .OrderByDescending(p => p.Id)
            .ToList();

        return View(programs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Restore(int id)
    {
        var archivedProgram = _context.ArchivedPrograms.FirstOrDefault(p => p.Id == id);
        if (archivedProgram != null)
        {
            using var transaction = _context.Database.BeginTransaction();

            var restoredProgram = new Programs
            {
                ProgramName = archivedProgram.ProgramName,
                ProgramType = archivedProgram.ProgramType,
                Description = archivedProgram.Description
            };

            // Restore with the original Program ID when possible.
            if (!_context.Programs.Any(p => p.Id == archivedProgram.OriginalProgramId))
            {
                restoredProgram.Id = archivedProgram.OriginalProgramId;
            }

            _context.Programs.Add(restoredProgram);
            _context.ArchivedPrograms.Remove(archivedProgram);
            _context.SaveChanges();

            transaction.Commit();
            TempData["ProgramsSuccess"] = "Program restored.";
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
            _context.Programs.Remove(program);
            _context.SaveChanges();
            TempData["ProgramsSuccess"] = "Program deleted.";
        }

        return RedirectToAction(nameof(Index));
    }
}
