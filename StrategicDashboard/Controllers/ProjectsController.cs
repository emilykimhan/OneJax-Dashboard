using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrategicDashboard.Models;
using StrategicDashboard.Services;

namespace StrategicDashboard.Controllers
{
    [Authorize(Roles = "Staff")]
    public class ProjectsController : Controller
    {
        private readonly ProjectsService _projects;
        private readonly ActivityLogService _activityLog;

        public ProjectsController(ProjectsService projects, ActivityLogService activityLog)
        {
            _projects = projects;
            _activityLog = activityLog;
        }

        public IActionResult Index()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var items = _projects.GetByOwner(username);
            return View(items);
        }

        public IActionResult Create()
        {
            return View(new Project());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Project project)
        {
            if (!ModelState.IsValid) return View(project);
            var username = User.Identity?.Name ?? string.Empty;
            project.OwnerUsername = username;
            var added = _projects.Add(project);
            _activityLog.Log(username, "Created Project", "Project", added.Id, notes: added.Title);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var project = _projects.Get(id);
            if (project == null) return NotFound();
            if (!IsOwner(project)) return Forbid();
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Project project)
        {
            var existing = _projects.Get(project.Id);
            if (existing == null) return NotFound();
            if (!IsOwner(existing)) return Forbid();
            if (!ModelState.IsValid) return View(project);
            // Keep owner unchanged
            project.OwnerUsername = existing.OwnerUsername;
            _projects.Update(project);
            _activityLog.Log(existing.OwnerUsername, "Updated Project", "Project", project.Id, notes: project.Title);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var project = _projects.Get(id);
            if (project == null) return NotFound();
            if (!IsOwner(project)) return Forbid();
            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var project = _projects.Get(id);
            if (project == null) return NotFound();
            if (!IsOwner(project)) return Forbid();
            _projects.Remove(id);
            _activityLog.Log(project.OwnerUsername, "Deleted Project", "Project", id, notes: project.Title);
            return RedirectToAction("Index");
        }

        private bool IsOwner(Project p)
        {
            var username = User.Identity?.Name ?? string.Empty;
            return string.Equals(p.OwnerUsername, username, StringComparison.OrdinalIgnoreCase);
        }
    }
}
