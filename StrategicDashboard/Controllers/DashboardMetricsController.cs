using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OneJaxDashboard.Services;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Controllers
{
    public class DashboardMetricsController : Controller
    {
        private readonly MetricsService _metricsService;

        public DashboardMetricsController(MetricsService metricsService)
        {
            _metricsService = metricsService;
        }

        // Integrated metrics management view
        public async Task<IActionResult> Index(string fiscalYear = "2025-2026")
        {
            // Initialize metrics if needed
            await _metricsService.SeedDashboardMetricsAsync();
            
            // Get metrics for all strategic goals
            var identityMetrics = await _metricsService.GetPublicMetricsAsync("Identity", fiscalYear);
            var communityMetrics = await _metricsService.GetPublicMetricsAsync("Community", fiscalYear);
            var financialMetrics = await _metricsService.GetPublicMetricsAsync("Financial", fiscalYear);
            var orgMetrics = await _metricsService.GetPublicMetricsAsync("Organizational", fiscalYear);

            ViewBag.FiscalYear = fiscalYear;
            ViewBag.IdentityMetrics = identityMetrics;
            ViewBag.CommunityMetrics = communityMetrics;
            ViewBag.FinancialMetrics = financialMetrics;
            ViewBag.OrganizationalMetrics = orgMetrics;

            return View();
        }

        // Quick update endpoint for AJAX calls - Requires Authentication
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> QuickUpdate(int metricId, decimal currentValue)
        {
            // Additional check to ensure user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Authentication required to edit metrics." });
            }
            
            try
            {
                await _metricsService.UpdateManualMetricAsync(metricId, currentValue);
                return Json(new { success = true, message = "Metric updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Get metrics for a specific strategic goal (AJAX endpoint)
        [HttpGet]
        public async Task<JsonResult> GetGoalMetrics(string goalName, string fiscalYear = "2025-2026")
        {
            var metrics = await _metricsService.GetPublicMetricsAsync(goalName, fiscalYear);
            return Json(metrics);
        }
    }
}
