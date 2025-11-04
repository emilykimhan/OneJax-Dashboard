using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StrategicDashboard.Models;
using StrategicDashboard.Data;

namespace OneJaxDashboard.Controllers
{
    public class DataEntryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

    }
}