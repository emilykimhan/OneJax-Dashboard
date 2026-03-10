using System.Text.Json;

namespace OneJaxDashboard.Services;

public sealed class DashboardNotesStore : IDashboardNotesStore
{
    private readonly string _notesPath;
    private readonly object _lock = new();

    public DashboardNotesStore(IWebHostEnvironment env)
    {
        var appDataDir = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(appDataDir);
        _notesPath = Path.Combine(appDataDir, "dashboard-notes.json");
    }

    public string? Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        var normalized = NormalizeKey(key);

        lock (_lock)
        {
            var dict = ReadAllUnsafe();
            return dict.TryGetValue(normalized, out var value) ? value : null;
        }
    }

    public void Set(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        var normalized = NormalizeKey(key);
        var trimmed = (value ?? string.Empty).Trim();

        lock (_lock)
        {
            var dict = ReadAllUnsafe();
            dict[normalized] = trimmed;
            WriteAllUnsafe(dict);
        }
    }

    private Dictionary<string, string> ReadAllUnsafe()
    {
        if (!File.Exists(_notesPath))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var json = File.ReadAllText(_notesPath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return dict ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            // If the file is corrupted, don't take down the dashboard.
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private void WriteAllUnsafe(Dictionary<string, string> dict)
    {
        var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_notesPath, json);
    }

    private static string NormalizeKey(string key)
    {
        return key.Trim().ToLowerInvariant();
    }
}

