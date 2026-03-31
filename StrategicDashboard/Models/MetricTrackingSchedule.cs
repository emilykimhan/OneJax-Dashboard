using System.Globalization;

namespace OneJaxDashboard.Models
{
    public static class MetricTrackingSchedule
    {
        // Central place to manage when each dashboard metric should begin counting
        // toward summary progress. These start years were inferred from the strategic
        // plan screenshots and can be adjusted without changing the database or forms.
        private static readonly IReadOnlyDictionary<string, string> StartFiscalYears =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Earned Media Placements"] = "2025-2026",
                ["Website Traffic"] = "2025-2026",
                ["Geographic Reach"] = "2025-2026",
                ["Community Perception Survey"] = "2025-2026",
                ["Strategic Plan Completion"] = "2025-2026",
                ["Interfaith Events Hosted"] = "2025-2026",
                ["Youth Attendance Growth"] = "2025-2026",
                ["Cross-Sector Collaborations"] = "2025-2026",
                ["Faith Representation"] = "2025-2026",
                ["Event Satisfaction"] = "2025-2026",
                ["Clergy Network Growth"] = "2025-2026",
                ["Donor Engagement Events"] = "2025-2026",
                ["Donor Communication Satisfaction"] = "2025-2026",
                ["Staff Satisfaction Rating"] = "2026-2027",
                ["Professional Development Plans"] = "2026-2027",
                ["Board Recruitment"] = "2026-2027",
                ["Board Meeting Participation"] = "2026-2027",
                ["Board Self-Assessment"] = "2026-2027",
                ["Budget Revenue Tracking"] = "2026-2027",
                ["Fee-for-Service Income"] = "2026-2027",
                ["General Income Streams"] = "2026-2027",
                ["Participant Diversity"] = "2027-2028",
                ["First-Time Participants"] = "2027-2028",
                ["Volunteer Program Participation"] = "2027-2028"
            };

        public static string? GetStartFiscalYear(string metricName)
        {
            return StartFiscalYears.TryGetValue(metricName ?? string.Empty, out var startFiscalYear)
                ? startFiscalYear
                : null;
        }

        public static bool IsScheduledForFiscalYear(string metricName, string? fiscalYear)
        {
            if (string.IsNullOrWhiteSpace(metricName)
                || string.IsNullOrWhiteSpace(fiscalYear)
                || string.Equals(fiscalYear, "All Years", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var startFiscalYear = GetStartFiscalYear(metricName);
            if (string.IsNullOrWhiteSpace(startFiscalYear))
            {
                return true;
            }

            return TryParseFiscalYearEnd(fiscalYear, out var selectedFiscalYearEnd)
                && TryParseFiscalYearEnd(startFiscalYear, out var startFiscalYearEnd)
                && selectedFiscalYearEnd >= startFiscalYearEnd;
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
