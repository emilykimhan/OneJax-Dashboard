using System.Globalization;

namespace OneJaxDashboard.Models
{
    public enum MetricTrackingType
    {
        Annual,
        Cumulative,
        Milestone,
        YearOverYear
    }

    public static class MetricTrackingSchedule
    {
        public sealed record MetricSchedule(
            string StartFiscalYear,
            string? EndFiscalYear,
            MetricTrackingType TrackingType);

        // Central place to manage which fiscal years each metric should count toward
        // by default. This keeps old one-time metrics from dragging down future annual
        // summaries while still allowing repeated metrics to reset each fiscal year.
        private static readonly IReadOnlyDictionary<string, MetricSchedule> Schedules =
            new Dictionary<string, MetricSchedule>(StringComparer.OrdinalIgnoreCase)
            {
                ["Earned Media Placements"] = new("2025-2026", "2026-2027", MetricTrackingType.Cumulative),
                ["Website Traffic"] = new("2025-2026", null, MetricTrackingType.YearOverYear),
                ["Geographic Reach"] = new("2025-2026", "2025-2026", MetricTrackingType.YearOverYear),
                ["Community Perception Survey"] = new("2025-2026", null, MetricTrackingType.Annual),
                ["Milestone Achievement"] = new("2025-2026", "2025-2026", MetricTrackingType.Milestone),
                ["Social Media Engagement"] = new("2025-2026", null, MetricTrackingType.YearOverYear),
                ["Interfaith Events Hosted"] = new("2025-2026", "2025-2026", MetricTrackingType.Annual),
                ["Youth Attendance Growth"] = new("2025-2026", null, MetricTrackingType.YearOverYear),
                ["Cross-Sector Collaborations"] = new("2025-2026", "2026-2027", MetricTrackingType.Cumulative),
                ["Faith Representation"] = new("2025-2026", null, MetricTrackingType.Annual),
                ["Event Satisfaction"] = new("2025-2026", null, MetricTrackingType.Annual),
                ["Clergy Network Growth"] = new("2025-2026", "2025-2026", MetricTrackingType.YearOverYear),
                ["Donor Engagement Events"] = new("2025-2026", null, MetricTrackingType.Annual),
                ["Donor Communication Satisfaction"] = new("2025-2026", null, MetricTrackingType.Annual),
                ["Staff Satisfaction Rating"] = new("2026-2027", null, MetricTrackingType.Annual),
                ["Professional Development Plans"] = new("2026-2027", null, MetricTrackingType.Annual),
                ["Strategic Plan Completion"] = new("2026-2027", "2026-2027", MetricTrackingType.Milestone),
                ["Budget Revenue Tracking"] = new("2025-2026", "2025-2026", MetricTrackingType.Annual),
                ["Fee-for-Service Income"] = new("2025-2026", "2026-2027", MetricTrackingType.Cumulative),
                ["General Income Streams"] = new("2026-2027", "2026-2027", MetricTrackingType.Cumulative),
                ["BoardRecruitment"] = new("2025-2026", null, MetricTrackingType.Annual),
                ["Board Recruitment"] = new("2025-2026", null, MetricTrackingType.Annual),
                ["Board Meeting Participation"] = new("2025-2026", null, MetricTrackingType.Annual),
                ["Board Self-Assessment"] = new("2025-2026", null, MetricTrackingType.Annual),
                ["Participant Diversity"] = new("2027-2028", null, MetricTrackingType.YearOverYear),
                ["First-Time Participants"] = new("2027-2028", null, MetricTrackingType.Annual),
                ["Volunteer Program Participation"] = new("2027-2028", null, MetricTrackingType.Cumulative)
            };

        public static MetricSchedule? GetSchedule(string metricName)
        {
            return Schedules.TryGetValue(metricName ?? string.Empty, out var schedule)
                ? schedule
                : null;
        }

        public static string? GetStartFiscalYear(string metricName)
        {
            return GetSchedule(metricName)?.StartFiscalYear;
        }

        public static string? GetEndFiscalYear(string metricName)
        {
            return GetSchedule(metricName)?.EndFiscalYear;
        }

        public static MetricTrackingType GetTrackingType(string metricName)
        {
            return GetSchedule(metricName)?.TrackingType ?? MetricTrackingType.Annual;
        }

        public static string GetInactiveReason(string metricName, string? fiscalYear)
        {
            var schedule = GetSchedule(metricName);
            if (schedule == null)
            {
                return "This metric does not count toward the selected fiscal year.";
            }

            if (string.IsNullOrWhiteSpace(fiscalYear)
                || string.Equals(fiscalYear, "All Years", StringComparison.OrdinalIgnoreCase)
                || IsScheduledForFiscalYear(metricName, fiscalYear))
            {
                return string.Empty;
            }

            if (TryParseFiscalYearEnd(fiscalYear, out var selectedFiscalYearEnd)
                && TryParseFiscalYearEnd(schedule.StartFiscalYear, out var startFiscalYearEnd)
                && selectedFiscalYearEnd < startFiscalYearEnd)
            {
                return $"Starts in FY {schedule.StartFiscalYear}.";
            }

            if (!string.IsNullOrWhiteSpace(schedule.EndFiscalYear)
                && TryParseFiscalYearEnd(fiscalYear ?? string.Empty, out selectedFiscalYearEnd)
                && TryParseFiscalYearEnd(schedule.EndFiscalYear, out var endFiscalYearEnd)
                && selectedFiscalYearEnd > endFiscalYearEnd)
            {
                return $"Ended after FY {schedule.EndFiscalYear}.";
            }

            return "This metric does not count toward the selected fiscal year.";
        }

        public static bool IsScheduledForFiscalYear(string metricName, string? fiscalYear)
        {
            if (string.IsNullOrWhiteSpace(metricName)
                || string.IsNullOrWhiteSpace(fiscalYear)
                || string.Equals(fiscalYear, "All Years", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var schedule = GetSchedule(metricName);
            if (schedule == null)
            {
                return true;
            }

            if (!TryParseFiscalYearEnd(fiscalYear, out var selectedFiscalYearEnd)
                || !TryParseFiscalYearEnd(schedule.StartFiscalYear, out var startFiscalYearEnd))
            {
                return false;
            }

            if (selectedFiscalYearEnd < startFiscalYearEnd)
            {
                return false;
            }

            return string.IsNullOrWhiteSpace(schedule.EndFiscalYear)
                || (TryParseFiscalYearEnd(schedule.EndFiscalYear, out var endFiscalYearEnd)
                    && selectedFiscalYearEnd <= endFiscalYearEnd);
        }

        private static bool TryParseFiscalYearEnd(string fiscalYear, out int fiscalYearEnd)
        {
            fiscalYearEnd = 0;
            if (string.IsNullOrWhiteSpace(fiscalYear))
            {
                return false;
            }

            var normalized = fiscalYear.Trim();
            var separators = new[] { "-", "–", "—", "/", "\\" };
            var parts = normalized.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length == 2
                && TryParseYear(parts[0], out var startYear)
                && TryParseYear(parts[1], out var endYear)
                && endYear == startYear + 1)
            {
                fiscalYearEnd = endYear;
                return true;
            }

            return TryParseYear(normalized, out fiscalYearEnd);
        }

        private static bool TryParseYear(string value, out int year)
        {
            year = 0;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                return false;
            }

            year = parsed < 100 ? 2000 + parsed : parsed;
            return true;
        }
    }
}
