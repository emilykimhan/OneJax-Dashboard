// Service for managing dashboard metrics configuration and calculations
// Emily - Dashboard metrics management

using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace OneJaxDashboard.Services
{
    public class MetricsService
    {
        private readonly ApplicationDbContext _context;

        public MetricsService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all public metrics for a specific strategic goal and fiscal year
        public async Task<List<GoalMetric>> GetPublicMetricsAsync(string strategicGoalName, string fiscalYear = "2025-2026")
        {
            var strategicGoal = await _context.StrategicGoals
                .FirstOrDefaultAsync(g => g.Name.Contains(strategicGoalName));
                
            if (strategicGoal == null) return new List<GoalMetric>();

            return await _context.GoalMetrics
                .Where(m => m.StrategicGoalId == strategicGoal.Id 
                        && m.IsPublic 
                        && m.FiscalYear == fiscalYear)
                .ToListAsync();
        }

        // Get all metrics (including internal) for authenticated users
        public async Task<List<GoalMetric>> GetAllMetricsAsync(string strategicGoalName, string fiscalYear = "2025-2026")
        {
            var strategicGoal = await _context.StrategicGoals
                .FirstOrDefaultAsync(g => g.Name.Contains(strategicGoalName));
                
            if (strategicGoal == null) return new List<GoalMetric>();

            return await _context.GoalMetrics
                .Where(m => m.StrategicGoalId == strategicGoal.Id 
                        && m.FiscalYear == fiscalYear)
                .ToListAsync();
        }

        // Update a manual entry metric
        public async Task UpdateManualMetricAsync(int metricId, decimal currentValue)
        {
            var metric = await _context.GoalMetrics.FindAsync(metricId);
            if (metric != null && metric.DataSource == "Manual")
            {
                metric.CurrentValue = currentValue;
                await _context.SaveChangesAsync();
            }
        }

        // Create a new dashboard metric
        public async Task<GoalMetric> CreateMetricAsync(GoalMetric metric)
        {
            _context.GoalMetrics.Add(metric);
            await _context.SaveChangesAsync();
            return metric;
        }

        // Initialize all your required metrics for the dashboard
        public async Task SeedDashboardMetricsAsync()
        {
            // Check if metrics already exist
            if (await _context.GoalMetrics.AnyAsync(m => m.DataSource != null))
                return; // Already seeded

            var strategicGoals = await _context.StrategicGoals.ToListAsync();
            var metrics = new List<GoalMetric>();

            // Identity/Value Proposition Metrics
            var identityGoal = strategicGoals.FirstOrDefault(g => g.Name.Contains("Identity"));
            if (identityGoal != null)
            {
                metrics.AddRange(new[]
                {
                    new GoalMetric
                    {
                        Name = "Earned Media Placements",
                        Description = "Media mentions achieved since July 2026",
                        StrategicGoalId = identityGoal.Id,
                        Target = "12",
                        CurrentValue = 0,
                        Unit = "placements",
                        DataSource = "Form",
                        MetricType = "Count",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-12-01")
                    },
                    new GoalMetric
                    {
                        Name = "Website Traffic Q1",
                        Description = "Website clicks July-September",
                        StrategicGoalId = identityGoal.Id,
                        Target = "1000",
                        CurrentValue = 0,
                        Unit = "clicks",
                        DataSource = "Form",
                        MetricType = "Quarterly",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-09-30")
                    },
                    new GoalMetric
                    {
                        Name = "Community Perception Survey",
                        Description = "Percentage identifying OneJax as trusted leader",
                        StrategicGoalId = identityGoal.Id,
                        Target = "70",
                        CurrentValue = 0,
                        Unit = "%",
                        DataSource = "Manual",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2025-12-31")
                    },
                    new GoalMetric
                    {
                        Name = "Key Plan Milestones",
                        Description = "Content calendar, press releases, brand messaging completion",
                        StrategicGoalId = identityGoal.Id,
                        Target = "75",
                        CurrentValue = 0,
                        Unit = "%",
                        DataSource = "Manual",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-01-01")
                    }
                });
            }

            // Community Engagement Metrics
            var communityGoal = strategicGoals.FirstOrDefault(g => g.Name.Contains("Community"));
            if (communityGoal != null)
            {
                metrics.AddRange(new[]
                {
                    new GoalMetric
                    {
                        Name = "Joint Initiative Satisfaction",
                        Description = "Partner satisfaction with joint initiatives",
                        StrategicGoalId = communityGoal.Id,
                        Target = "85",
                        CurrentValue = 0,
                        Unit = "%",
                        DataSource = "Form",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2027-06-30")
                    },
                    new GoalMetric
                    {
                        Name = "Cross-Sector Collaborations",
                        Description = "Number of unique cross-sector collaborations",
                        StrategicGoalId = communityGoal.Id,
                        Target = "10",
                        CurrentValue = 3,
                        Unit = "collaborations",
                        DataSource = "Manual",
                        MetricType = "Count",
                        IsPublic = true,
                        FiscalYear = "2026-2027",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2027-06-30")
                    },
                    new GoalMetric
                    {
                        Name = "Interfaith Events Hosted",
                        Description = "Number of interfaith collaborative events",
                        StrategicGoalId = communityGoal.Id,
                        Target = "4",
                        CurrentValue = 0,
                        Unit = "events",
                        DataSource = "Form",
                        MetricType = "Count",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-06-30")
                    },
                    new GoalMetric
                    {
                        Name = "Youth Program Satisfaction",
                        Description = "Average satisfaction across all youth programs",
                        StrategicGoalId = communityGoal.Id,
                        Target = "85",
                        CurrentValue = 0,
                        Unit = "%",
                        DataSource = "Form",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-06-30")
                    }
                });
            }

            // Financial Stability Metrics
            var financialGoal = strategicGoals.FirstOrDefault(g => g.Name.Contains("Financial"));
            if (financialGoal != null)
            {
                metrics.AddRange(new[]
                {
                    new GoalMetric
                    {
                        Name = "Fee-for-Service Revenue",
                        Description = "Cumulative earned income from workshops",
                        StrategicGoalId = financialGoal.Id,
                        Target = "50000",
                        CurrentValue = 0,
                        Unit = "$",
                        DataSource = "Form",
                        MetricType = "Currency",
                        IsPublic = true,
                        FiscalYear = "2026-2027",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2027-06-30")
                    },
                    new GoalMetric
                    {
                        Name = "Budget Variance",
                        Description = "Variance from annual budget",
                        StrategicGoalId = financialGoal.Id,
                        Target = "10",
                        CurrentValue = 0,
                        Unit = "%",
                        DataSource = "Calculated",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-06-30")
                    },
                    new GoalMetric
                    {
                        Name = "Donor Communication Satisfaction",
                        Description = "Annual donor satisfaction with communications",
                        StrategicGoalId = financialGoal.Id,
                        Target = "85",
                        CurrentValue = 0,
                        Unit = "%",
                        DataSource = "Manual",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-06-30")
                    }
                });
            }

            // Organizational Building Metrics
            var orgGoal = strategicGoals.FirstOrDefault(g => g.Name.Contains("Organizational"));
            if (orgGoal != null)
            {
                metrics.AddRange(new[]
                {
                    new GoalMetric
                    {
                        Name = "Staff Satisfaction (Public)",
                        Description = "Overall staff satisfaction percentage",
                        StrategicGoalId = orgGoal.Id,
                        Target = "85",
                        CurrentValue = 0,
                        Unit = "%",
                        DataSource = "Calculated",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2026-2027",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2027-06-30")
                    },
                    new GoalMetric
                    {
                        Name = "Professional Development Participation",
                        Description = "Number of staff participating in development",
                        StrategicGoalId = orgGoal.Id,
                        Target = "100",
                        CurrentValue = 0,
                        Unit = "%",
                        DataSource = "Calculated",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2026-2027",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2027-06-30")
                    },
                    new GoalMetric
                    {
                        Name = "Board Member Count",
                        Description = "Total number of board members",
                        StrategicGoalId = orgGoal.Id,
                        Target = "12",
                        CurrentValue = 0,
                        Unit = "members",
                        DataSource = "Manual",
                        MetricType = "Count",
                        IsPublic = true,
                        FiscalYear = "2026-2027",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-07-01")
                    },
                    new GoalMetric
                    {
                        Name = "Board Self-Assessment",
                        Description = "Average board engagement and impact scores",
                        StrategicGoalId = orgGoal.Id,
                        Target = "85",
                        CurrentValue = 0,
                        Unit = "%",
                        DataSource = "Manual",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2026-2027",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2027-06-30")
                    },
                    new GoalMetric
                    {
                        Name = "Board Meeting Attendance",
                        Description = "Percentage attendance at board meetings",
                        StrategicGoalId = orgGoal.Id,
                        Target = "90",
                        CurrentValue = 0,
                        Unit = "%",
                        DataSource = "Form",
                        MetricType = "Percentage",
                        IsPublic = false, // Internal only
                        FiscalYear = "2026-2027",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2027-06-30")
                    }
                });
            }

            _context.GoalMetrics.AddRange(metrics);
            await _context.SaveChangesAsync();
        }
    }
}
