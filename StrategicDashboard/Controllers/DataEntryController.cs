using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StrategicDashboard.Models;
using OneJaxDashboard.Data;
using System;

namespace OneJaxDashboard.Controllers
{
    public class DataEntryController : Controller
    {
        private readonly DbContext _context;

        public DataEntryController(DbContext context)
        {
            _context = context;
        }

    }
}