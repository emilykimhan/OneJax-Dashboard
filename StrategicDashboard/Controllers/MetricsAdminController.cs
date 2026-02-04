// Controller for managing dashboard metrics
// Emily - Admin interface for metrics management

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OneJaxDashboard.Services;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Controllers
{
    [Authorize]
    public class MetricsAdminController : Controller
    {
        private readonly MetricsService _metricsService;

        public MetricsAdminController(MetricsService metricsService)
        {
            _metricsService = metricsService;
        }

        // Admin dashboard for metrics management
        public async Task<IActionResult> Index()
        {
            // Initialize metrics if not done already
            await _metricsService.SeedDashboardMetricsAsync();
            
            // Get all metrics grouped by strategic goal
            var identityMetrics = await _metricsService.GetAllMetricsAsync("Identity", "2025-2026");
            var communityMetrics = await _metricsService.GetAllMetricsAsync("Community", "2025-2026");
            var financialMetrics = await _metricsService.GetAllMetricsAsync("Financial", "2025-2026");
            var orgMetrics = await _metricsService.GetAllMetricsAsync("Organizational", "2026-2027");

            ViewBag.IdentityMetrics = identityMetrics;
            ViewBag.CommunityMetrics = communityMetrics;
            ViewBag.FinancialMetrics = financialMetrics;
            ViewBag.OrganizationalMetrics = orgMetrics;

            return View();
        }

        // Update manual entry metrics
        [HttpPost]
        public async Task<IActionResult> UpdateMetric(int metricId, decimal currentValue, string returnUrl = null)
        {
            await _metricsService.UpdateManualMetricAsync(metricId, currentValue);
            
            // If called from DashboardMetrics, redirect there with success indicator
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("DashboardMetrics"))
            {
                return Redirect($"{returnUrl}?updated=true");
            }
            
            TempData["Success"] = "Metric updated successfully!";
            return RedirectToAction("Index");
        }

        // Initialize/seed all dashboard metrics
        [HttpPost]
        public async Task<IActionResult> SeedMetrics()
        {
            await _metricsService.SeedDashboardMetricsAsync();
            TempData["Success"] = "Dashboard metrics have been initialized with your complete metrics list!";
            return RedirectToAction("Index");
        }

        // Get metrics for a specific fiscal year (AJAX endpoint)
        [HttpGet]
        public async Task<JsonResult> GetMetricsByFiscalYear(string fiscalYear = "2025-2026")
        {
            var identityMetrics = await _metricsService.GetAllMetricsAsync("Identity", fiscalYear);
            var communityMetrics = await _metricsService.GetAllMetricsAsync("Community", fiscalYear);
            var financialMetrics = await _metricsService.GetAllMetricsAsync("Financial", fiscalYear);
            var orgMetrics = await _metricsService.GetAllMetricsAsync("Organizational", fiscalYear);

            return Json(new {
                identity = identityMetrics,
                community = communityMetrics,
                financial = financialMetrics,
                organizational = orgMetrics
            });
        }
    }
}
