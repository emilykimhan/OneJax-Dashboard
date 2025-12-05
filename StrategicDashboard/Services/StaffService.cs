using OneJaxDashboard.Models;
//Talijah's 
namespace OneJaxDashboard.Services
{
    public class StaffService
    {
        private static List<Staff> _staffList = new List<Staff>();

        public IEnumerable<Staff> GetAll() => _staffList;
        public Staff? Get(int id) => _staffList.FirstOrDefault(s => s.Id == id);
        public Staff? GetByUsername(string username) => _staffList.FirstOrDefault(s => s.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        public void Add(Staff staff)
        {
            staff.Id = _staffList.Count > 0 ? _staffList.Max(s => s.Id) + 1 : 1;
            _staffList.Add(staff);
        }
        public void Update(Staff staff)
        {
            var existing = Get(staff.Id);
            if (existing != null)
            {
                existing.Username = staff.Username;
                existing.Password = staff.Password;
                existing.FullName = staff.FullName;
                existing.Email = staff.Email;
            }
        }
        public void Remove(int id) => _staffList.RemoveAll(s => s.Id == id);

        // For staff self-service: update profile (full name, email). Username/password unchanged here.
        public void UpdateProfile(string username, string fullName, string email)
        {
            var existing = GetByUsername(username);
            if (existing != null)
            {
                existing.FullName = fullName;
                existing.Email = email;
            }
        }
    }
}