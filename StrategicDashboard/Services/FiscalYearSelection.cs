using Microsoft.AspNetCore.Http;

namespace OneJaxDashboard.Services;

public static class FiscalYearSelection
{
    private const string FiscalYearCookieName = "onejax_dashboard_fiscal_year";
    private const string AllYearsSentinel = "__ALL__";

    public static string GetCurrentDashboardFiscalYearLabel(DateTime? currentDate = null)
    {
        var date = currentDate ?? DateTime.Today;
        var startYear = date.Month >= 7 ? date.Year : date.Year - 1;
        return $"{startYear}-{startYear + 1}";
    }

    public static string GetCurrentEventsFiscalYearLabel(DateTime? currentDate = null)
    {
        return ToEventsFormat(GetCurrentDashboardFiscalYearLabel(currentDate));
    }

    public static string ResolveDashboardFiscalYear(HttpRequest request, string? requestedFiscalYear)
    {
        if (request.Query.ContainsKey("fiscalYear"))
        {
            return NormalizeDashboardLabel(requestedFiscalYear) ?? string.Empty;
        }

        if (request.Cookies.TryGetValue(FiscalYearCookieName, out var cookieValue))
        {
            if (string.Equals(cookieValue, AllYearsSentinel, StringComparison.Ordinal))
            {
                return string.Empty;
            }

            var normalizedCookieValue = NormalizeDashboardLabel(cookieValue);
            if (!string.IsNullOrWhiteSpace(normalizedCookieValue))
            {
                return normalizedCookieValue;
            }
        }

        return GetCurrentDashboardFiscalYearLabel();
    }

    public static string ResolveEventsFiscalYear(HttpRequest request, string? requestedFiscalYear)
    {
        if (request.Query.ContainsKey("fy"))
        {
            return NormalizeEventsLabel(requestedFiscalYear) ?? string.Empty;
        }

        if (request.Cookies.TryGetValue(FiscalYearCookieName, out var cookieValue))
        {
            if (string.Equals(cookieValue, AllYearsSentinel, StringComparison.Ordinal))
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(cookieValue))
            {
                var normalizedCookieValue = NormalizeEventsLabel(cookieValue);
                if (!string.IsNullOrWhiteSpace(normalizedCookieValue))
                {
                    return normalizedCookieValue;
                }
            }
        }

        return GetCurrentEventsFiscalYearLabel();
    }

    public static void PersistSelection(HttpResponse response, string? fiscalYear)
    {
        var cookieValue = string.IsNullOrWhiteSpace(fiscalYear)
            ? AllYearsSentinel
            : NormalizeDashboardLabel(fiscalYear) ?? fiscalYear.Trim();

        response.Cookies.Append(
            FiscalYearCookieName,
            cookieValue,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(90),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = false
            });
    }

    public static string ToDashboardFormat(string? fiscalYear)
    {
        return NormalizeDashboardLabel(fiscalYear) ?? string.Empty;
    }

    public static string ToEventsFormat(string? fiscalYear)
    {
        return NormalizeEventsLabel(fiscalYear) ?? string.Empty;
    }

    public static string? NormalizeDashboardLabel(string? fiscalYear)
    {
        return TryParseFiscalYear(fiscalYear, out var startYear, out var endYear)
            ? $"{startYear}-{endYear}"
            : null;
    }

    public static string? NormalizeEventsLabel(string? fiscalYear)
    {
        return TryParseFiscalYear(fiscalYear, out var startYear, out var endYear)
            ? $"{startYear}/{endYear}"
            : null;
    }

    public static bool TryParseFiscalYear(string? fiscalYear, out int startYear, out int endYear)
    {
        startYear = 0;
        endYear = 0;

        if (string.IsNullOrWhiteSpace(fiscalYear))
        {
            return false;
        }

        var normalized = fiscalYear.Trim();
        if (normalized.StartsWith("FY ", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[3..].Trim();
        }
        else if (normalized.StartsWith("FY", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[2..].Trim();
        }

        var parts = normalized.Split(new[] { '-', '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        if (!TryParseFiscalYearPart(parts[0], out startYear) || !TryParseFiscalYearPart(parts[1], out endYear))
        {
            return false;
        }

        return endYear == startYear + 1;
    }

    private static bool TryParseFiscalYearPart(string value, out int year)
    {
        year = 0;

        if (!int.TryParse(value, out var parsedYear))
        {
            return false;
        }

        year = value.Length == 2 ? 2000 + parsedYear : parsedYear;
        return year is >= 2000 and <= 2100;
    }
}
