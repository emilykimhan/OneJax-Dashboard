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

        // Update existing website traffic metrics to new format
        private async Task UpdateWebsiteTrafficMetricAsync()
        {
            var oldWebsiteMetric = await _context.GoalMetrics.FirstOrDefaultAsync(m => m.Name == "Website Traffic Q1");
            if (oldWebsiteMetric != null)
            {
                // Update to new name and properties
                oldWebsiteMetric.Name = "Website Traffic (Annual)";
                oldWebsiteMetric.Description = "Total website clicks across all quarters";
                oldWebsiteMetric.Target = "4000";
                oldWebsiteMetric.MetricType = "Annual";
                oldWebsiteMetric.TargetDate = DateTime.Parse("2026-06-30");
                
                await _context.SaveChangesAsync();
            }
        }

        // Initialize all your required metrics for the dashboard
        public async Task SeedDashboardMetricsAsync()
        {
            // First, update any existing website traffic metrics to the new format
            await UpdateWebsiteTrafficMetricAsync();
            
            // Check if metrics already exist
            if (await _context.GoalMetrics.AnyAsync(m => m.DataSource != null))
                return; // Already seeded

            var strategicGoals = await _context.StrategicGoals.ToListAsync();
            var metrics = new List<GoalMetric>();

            // Identity/Value Proposition Metrics - Website Traffic (graph), Media Placements (card), Community Perception Survey, Program Demographics (geographic heatmap), Framework Development Plan, Framework Compliance
            var identityGoal = strategicGoals.FirstOrDefault(g => g.Name.Contains("Identity"));
            if (identityGoal != null)
            {
                metrics.AddRange(new[]
                {
                    new GoalMetric
                    {
                        Name = "Website Traffic (Annual)",
                        Description = "Total website clicks with quarterly breakdown showing growth trends",
                        StrategicGoalId = identityGoal.Id,
                        Target = "50000",
                        CurrentValue = 37500,
                        Unit = "clicks",
                        DataSource = "Form",
                        MetricType = "Annual",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-06-30"),
                        Q1Value = 8000,
                        Q2Value = 9500,
                        Q3Value = 12000,
                        Q4Value = 8000
                    },
                    new GoalMetric
                    {
                        Name = "Media Placements",
                        Description = "Number of positive media placements and coverage per year",
                        StrategicGoalId = identityGoal.Id,
                        Target = "120",
                        CurrentValue = 85,
                        Unit = "placements",
                        DataSource = "Form",
                        MetricType = "Count",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-06-30"),
                        Q1Value = 20,
                        Q2Value = 22,
                        Q3Value = 25,
                        Q4Value = 18
                    },
                    new GoalMetric
                    {
                        Name = "Community Perception Survey",
                        Description = "Percentage identifying OneJax as trusted community leader",
                        StrategicGoalId = identityGoal.Id,
                        Target = "75",
                        CurrentValue = 68.5m,
                        Unit = "%",
                        DataSource = "Form",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-06-30"),
                        Q1Value = 65,
                        Q2Value = 67,
                        Q3Value = 70,
                        Q4Value = 72
                    },
                    new GoalMetric
                    {
                        Name = "Program Demographics",
                        Description = "Geographic distribution and demographic data of program participants",
                        StrategicGoalId = identityGoal.Id,
                        Target = "100",
                        CurrentValue = 92,
                        Unit = "%",
                        DataSource = "Form",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-06-30"),
                        Q1Value = 88,
                        Q2Value = 90,
                        Q3Value = 94,
                        Q4Value = 96
                    },
                    new GoalMetric
                    {
                        Name = "Framework Development Plan",
                        Description = "Progress on developing organizational framework and strategic plans",
                        StrategicGoalId = identityGoal.Id,
                        Target = "100",
                        CurrentValue = 75,
                        Unit = "%",
                        DataSource = "Form",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-06-30"),
                        Q1Value = 25,
                        Q2Value = 50,
                        Q3Value = 75,
                        Q4Value = 90
                    },
                    new GoalMetric
                    {
                        Name = "Framework Compliance",
                        Description = "Adherence to organizational framework and compliance standards",
                        StrategicGoalId = identityGoal.Id,
                        Target = "95",
                        CurrentValue = 88,
                        Unit = "%",
                        DataSource = "Form",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2025-2026",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2026-06-30"),
                        Q1Value = 85,
                        Q2Value = 87,
                        Q3Value = 90,
                        Q4Value = 92
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

            // Financial Stability Metrics - Based on specific forms: Donor/Honoree Engagement, Communication Satisfaction, Fee-for-Service Revenue, Earned Income Tracking, Annual Budget Tracking
            var financialGoal = strategicGoals.FirstOrDefault(g => g.Name.Contains("Financial"));
            if (financialGoal != null)
            {
                metrics.AddRange(new[]
                {
                    new GoalMetric
                    {
                        Name = "Donor/Honoree Engagement Rate",
                        Description = "Percentage of active donor engagement and honoree participation",
                        StrategicGoalId = financialGoal.Id,
                        Target = "80",
                        CurrentValue = 0,
                        Unit = "%",
                        DataSource = "Form",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2026-2027",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2027-06-30")
                    },
                    new GoalMetric
                    {
                        Name = "Communication Satisfaction",
                        Description = "Donor and stakeholder satisfaction with communications",
                        StrategicGoalId = financialGoal.Id,
                        Target = "85",
                        CurrentValue = 0,
                        Unit = "%",
                        DataSource = "Form",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2026-2027",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2027-06-30")
                    },
                    new GoalMetric
                    {
                        Name = "Fee-for-Service Revenue",
                        Description = "Cumulative earned income from workshops and services",
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
                        Name = "Earned Income Tracking",
                        Description = "Total earned income from all revenue-generating activities",
                        StrategicGoalId = financialGoal.Id,
                        Target = "75000",
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
                        Name = "Annual Budget Tracking",
                        Description = "Budget variance and adherence to annual financial plan",
                        StrategicGoalId = financialGoal.Id,
                        Target = "95",
                        CurrentValue = 0,
                        Unit = "%",
                        DataSource = "Form",
                        MetricType = "Percentage",
                        IsPublic = true,
                        FiscalYear = "2026-2027",
                        Status = "Active",
                        TargetDate = DateTime.Parse("2027-06-30")
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
                        Name = "Staff Satisfaction Rate",
                        Description = "Overall staff satisfaction percentage based on quarterly surveys",
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
                        Name = "Professional Development Activities",
                        Description = "Track employee professional development activities. Employees must participate in at least 1 opportunity for professional development annually across 2026 and 2027",
                        StrategicGoalId = orgGoal.Id,
                        Target = "100",
                        CurrentValue = 0,
                        Unit = "% participation",
                        DataSource = "Calculated",
                        MetricType = "Percentage",
                        IsPublic = true,
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
