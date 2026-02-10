// Purpose: Represents a specific metric/KPI being tracked for a strategic goal
// Usage: Tracks progress against targets with quarterly data support

//Emily
namespace OneJaxDashboard.Models
{
    public class GoalMetric
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int StrategicGoalId { get; set; }
        
        // Target and progress tracking
        public string Target { get; set; } = "";
        public decimal CurrentValue { get; set; }
        public string Unit { get; set; } = ""; // e.g., "placements", "%", "visitors"
        
        // Status and timing
        public string Status { get; set; } = "Active"; // Active, Completed, On Hold
        public DateTime TargetDate { get; set; }
        
        // Dashboard display properties
        public string DataSource { get; set; } = "Form"; // Form, Manual, Calculated
        public string MetricType { get; set; } = "Count"; // Count, Percentage, Currency, Quarterly
        public bool IsPublic { get; set; } = true; // Show on public dashboard
        public string FiscalYear { get; set; } = "2025-2026"; // Which fiscal year this applies to
        
        // Quarterly data (for metrics like website traffic)
        public decimal Q1Value { get; set; }
        public decimal Q2Value { get; set; }
        public decimal Q3Value { get; set; }
        public decimal Q4Value { get; set; }
        
        // Calculate progress percentage
        public decimal ProgressPercentage
        {
            get
            {
                if (string.IsNullOrEmpty(Target) || CurrentValue <= 0)
                    return 0;

                // Handle percentage units
                if (Unit == "%")
                {
                    if (decimal.TryParse(Target.Replace("%", ""), out decimal targetValue) && targetValue > 0)
                    {
                        return Math.Min(Math.Round((CurrentValue / targetValue) * 100, 1), 100);
                    }
                }
                
                // Handle all other numeric targets
                if (decimal.TryParse(Target, out decimal targetDecimal) && targetDecimal > 0)
                {
                    return Math.Min(Math.Round((CurrentValue / targetDecimal) * 100, 1), 100);
                }
                
                return 0;
            }
        }
    }
}
