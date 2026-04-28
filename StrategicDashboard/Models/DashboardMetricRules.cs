using System.Globalization;

namespace OneJaxDashboard.Models;

public static class DashboardMetricRules
{
    private static readonly HashSet<string> MilestoneMetrics = new(StringComparer.OrdinalIgnoreCase)
    {
        "Strategic Plan Completion",
        "Milestone Achievement",
        "Social Media Engagement"
    };

    private static readonly HashSet<string> GrowthMetrics = new(StringComparer.OrdinalIgnoreCase)
    {
        "Website Traffic",
        "Youth Attendance Growth",
        "Clergy Network Growth",
        "Participant Diversity"
    };

    private static readonly HashSet<string> SnapshotMetrics = new(StringComparer.OrdinalIgnoreCase)
    {
        "Community Perception Survey",
        "Staff Satisfaction Rating",
        "Board Meeting Participation",
        "Board Self-Assessment",
        "Donor Communication Satisfaction",
        "Faith Representation",
        "Event Satisfaction",
        "First-Time Participants",
        "Professional Development Plans"
    };

    private static readonly HashSet<string> CumulativeMetrics = new(StringComparer.OrdinalIgnoreCase)
    {
        "Earned Media Placements",
        "Geographic Reach",
        "Board Recruitment",
        "Volunteer Program Participation",
        "Budget Revenue Tracking",
        "Fee-for-Service Income",
        "General Income Streams",
        "Donor Engagement Events",
        "Interfaith Events Hosted",
        "Cross-Sector Collaborations"
    };

    private static readonly HashSet<string> OngoingMetrics = new(StringComparer.OrdinalIgnoreCase);

    public static bool IsScheduledMetric(GoalMetric metric, string? fiscalYear)
    {
        return metric != null
            && MetricTrackingSchedule.IsScheduledForFiscalYear(metric.Name, fiscalYear);
    }

    public static IEnumerable<GoalMetric> ScheduledMetrics(IEnumerable<GoalMetric>? metrics, string? fiscalYear)
    {
        return (metrics ?? Enumerable.Empty<GoalMetric>())
            .Where(metric => IsScheduledMetric(metric, fiscalYear));
    }

    public static bool TryParseTarget(string? target, out decimal targetValue)
    {
        targetValue = 0;
        if (string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        var cleaned = target
            .Replace("$", string.Empty, StringComparison.Ordinal)
            .Replace(",", string.Empty, StringComparison.Ordinal)
            .Replace("%", string.Empty, StringComparison.Ordinal)
            .Trim();

        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out targetValue)
            && targetValue > 0;
    }

    public static bool IsReportingMetric(GoalMetric metric, string? fiscalYear)
    {
        return metric != null
            && IsScheduledMetric(metric, fiscalYear)
            && !string.Equals(metric.Status, "Planning", StringComparison.OrdinalIgnoreCase)
            && TryParseTarget(metric.Target, out _);
    }

    public static IEnumerable<GoalMetric> ReportingMetrics(IEnumerable<GoalMetric>? metrics, string? fiscalYear)
    {
        return (metrics ?? Enumerable.Empty<GoalMetric>())
            .Where(metric => IsReportingMetric(metric, fiscalYear));
    }

    public static decimal GetMetricProgressPercentage(GoalMetric metric, decimal cap = 100m)
    {
        if (metric == null || !TryParseTarget(metric.Target, out var targetValue))
        {
            return 0;
        }

        var progress = Math.Round((metric.CurrentValue / targetValue) * 100m, 1);

        if (metric.HasSampleRequirement && !metric.HasSufficientSample)
        {
            var sampleRatio = metric.MinimumSampleSize.GetValueOrDefault() > 0
                ? (decimal)metric.SampleCount.GetValueOrDefault() / metric.MinimumSampleSize.GetValueOrDefault()
                : 0m;
            progress = Math.Round(progress * Math.Clamp(sampleRatio, 0m, 1m), 1);
        }

        return Math.Min(progress, cap);
    }

    public static bool IsMetricAtTarget(GoalMetric metric)
    {
        return metric != null
            && (!metric.HasSampleRequirement || metric.HasSufficientSample)
            && GetMetricProgressPercentage(metric, 100m) >= 100m;
    }

    public static decimal CalculateGoalProgress(IEnumerable<GoalMetric>? metrics, string? fiscalYear)
    {
        var progressValues = ScheduledMetrics(metrics, fiscalYear)
            .Select(metric => IsReportingMetric(metric, fiscalYear)
                ? GetMetricProgressPercentage(metric, 100m)
                : 0m)
            .ToList();

        return progressValues.Any() ? Math.Round(progressValues.Average(), 1) : 0m;
    }

    public static string GetSummaryCategory(GoalMetric metric)
    {
        var name = metric?.Name ?? string.Empty;

        if (MilestoneMetrics.Contains(name))
        {
            return "Milestone";
        }

        if (GrowthMetrics.Contains(name))
        {
            return "Growth";
        }

        if (SnapshotMetrics.Contains(name))
        {
            return "Snapshot";
        }

        if (CumulativeMetrics.Contains(name))
        {
            return "Cumulative";
        }

        if (OngoingMetrics.Contains(name))
        {
            return "Ongoing";
        }

        return "Snapshot";
    }

    public static bool CountsTowardOverallProgress(GoalMetric metric)
    {
        return !string.Equals(GetSummaryCategory(metric), "Ongoing", StringComparison.OrdinalIgnoreCase);
    }

    public static IEnumerable<GoalMetric> OverallProgressMetrics(IEnumerable<GoalMetric>? metrics, string? fiscalYear)
    {
        return ScheduledMetrics(metrics, fiscalYear)
            .Where(CountsTowardOverallProgress);
    }

    public static IEnumerable<MetricTypeData> BuildMetricDistribution(IEnumerable<GoalMetric>? metrics, string? fiscalYear)
    {
        return ScheduledMetrics(metrics, fiscalYear)
            .GroupBy(GetSummaryCategory)
            .OrderBy(group => group.Key)
            .Select(group => new MetricTypeData
            {
                Type = group.Key,
                Count = group.Count()
            })
            .ToList();
    }
}
