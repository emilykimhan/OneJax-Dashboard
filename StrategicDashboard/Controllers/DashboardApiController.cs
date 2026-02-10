using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/DashboardApi/events
        [HttpPost("events")]
        public async Task<IActionResult> AddEvent([FromBody] Event eventData)
        {
            try
            {
                _context.Events.Add(eventData);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Event added successfully", eventId = eventData.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error adding event: {ex.Message}" });
            }
        }

        // POST: api/DashboardApi/metrics
        [HttpPost("metrics")]
        public async Task<IActionResult> AddMetric([FromBody] GoalMetric metric)
        {
            try
            {
                _context.GoalMetrics.Add(metric);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Metric added successfully", metricId = metric.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error adding metric: {ex.Message}" });
            }
        }

        // GET: api/DashboardApi/summary
        [HttpGet("summary")]
        public IActionResult GetDashboardSummary()
        {
            try
            {
                var summary = new
                {
                    totalEvents = _context.Events.Count(),
                    totalMetrics = _context.GoalMetrics.Count(),
                    totalStaffSurveys = _context.StaffSurveys_22D.Count(),
                    totalProfDev = _context.ProfessionalDevelopments.Count(),
                    totalMediaPlacements = _context.MediaPlacements_3D.Count(),
                    totalWebsiteTraffic = _context.WebsiteTraffic.Count(),
                    lastUpdated = DateTime.Now
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: api/DashboardApi/bulk-import
        [HttpPost("bulk-import")]
        public async Task<IActionResult> BulkImport([FromBody] BulkImportRequest request)
        {
            try
            {
                var results = new BulkImportResults();

                // Import Events
                if (request.Events?.Any() == true)
                {
                    _context.Events.AddRange(request.Events);
                    results.EventsImported = request.Events.Count;
                }

                // Import Metrics
                if (request.Metrics?.Any() == true)
                {
                    _context.GoalMetrics.AddRange(request.Metrics);
                    results.MetricsImported = request.Metrics.Count;
                }

                await _context.SaveChangesAsync();
                results.Success = true;
                results.Message = "Bulk import completed successfully";

                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Bulk import failed: {ex.Message}" });
            }
        }
    }

    // Data transfer objects for API
    public class BulkImportRequest
    {
        public List<Event>? Events { get; set; }
        public List<GoalMetric>? Metrics { get; set; }
        public List<StrategicGoal>? Goals { get; set; }
    }

    public class BulkImportResults
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int EventsImported { get; set; }
        public int MetricsImported { get; set; }
        public int GoalsImported { get; set; }
    }
}
