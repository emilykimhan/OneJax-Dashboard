using OneJaxDashboard.Models;
//Talijah's
namespace OneJaxDashboard.Services
{
    public class ProjectsService
    {
        private static readonly List<Project> _projects = new();

        public IEnumerable<Project> GetByOwner(string username)
            => _projects.Where(p => string.Equals(p.OwnerUsername, username, StringComparison.OrdinalIgnoreCase));

        public Project? Get(int id) => _projects.FirstOrDefault(p => p.Id == id);

        public Project Add(Project project)
        {
            project.Id = _projects.Count > 0 ? _projects.Max(p => p.Id) + 1 : 1;
            _projects.Add(project);
            return project;
        }

        public void Update(Project project)
        {
            var existing = Get(project.Id);
            if (existing == null) return;
            existing.Title = project.Title;
            existing.Description = project.Description;
            existing.Status = project.Status;
            existing.StartDate = project.StartDate;
            existing.EndDate = project.EndDate;
        }

        public void Remove(int id) => _projects.RemoveAll(p => p.Id == id);
    }
}
