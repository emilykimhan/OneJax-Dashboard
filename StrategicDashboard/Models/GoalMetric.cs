// Purpose: Represents a specific metric/KPI being tracked for a strategic goal
// Usage: Tracks progress against targets with quarterly data support

//Emily
using System.ComponentModel.DataAnnotations.Schema;

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
        [Column(TypeName = "decimal(18,2)")]
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
        [Column(TypeName = "decimal(18,2)")]
        public decimal Q1Value { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Q2Value { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Q3Value { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Q4Value { get; set; }

        // Evidence/sample tracking is derived at runtime for dashboard display only.
        [NotMapped]
        public int? SampleCount { get; set; }
        [NotMapped]
        public int? MinimumSampleSize { get; set; }
        [NotMapped]
        public string SampleCountText { get; set; } = "";
        [NotMapped]
        public string SampleRequirementText { get; set; } = "";

        [NotMapped]
        public bool HasSampleRequirement => MinimumSampleSize.GetValueOrDefault() > 0;

        [NotMapped]
        public bool HasSufficientSample =>
            !HasSampleRequirement || SampleCount.GetValueOrDefault() >= MinimumSampleSize.GetValueOrDefault();

        // Calculate progress percentage
        [NotMapped]
        public decimal ProgressPercentage
        {
            get
            {
                if (string.IsNullOrEmpty(Target) || Target.Trim() == "0")
                    return 0;

                // Helper method to safely parse target values
                decimal SafeParseTarget(string target)
                {
                    if (string.IsNullOrEmpty(target))
                        return 0;
                        
                    // Remove common currency symbols and formatting characters
                    var cleanTarget = target
                        .Replace("$", "")
                        .Replace(",", "")
                        .Replace("%", "")
                        .Replace(" ", "")
                        .Trim();
                        
                    return decimal.TryParse(cleanTarget, out decimal value) ? value : 0;
                }

                var targetValue = SafeParseTarget(Target);
                if (targetValue == 0)
                    return 0;

                var progressPercent = Math.Round((CurrentValue / targetValue) * 100, 1);

                if (HasSampleRequirement && !HasSufficientSample)
                {
                    var sampleRatio = MinimumSampleSize.GetValueOrDefault() > 0
                        ? (decimal)SampleCount.GetValueOrDefault() / MinimumSampleSize.GetValueOrDefault()
                        : 0;
                    progressPercent = Math.Round(progressPercent * Math.Clamp(sampleRatio, 0m, 1m), 1);
                }
                
                // Cap at 150% to prevent extremely high values from skewing averages
                return Math.Min(progressPercent, 150);
            }
        }
    }
}
