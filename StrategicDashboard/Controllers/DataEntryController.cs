using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using StrategicDashboard.Models;
using StrategicDashboard.Data;

namespace StrategicDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class DataEntryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

    }
}