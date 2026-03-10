namespace OneJaxDashboard.Services;

public interface IDashboardNotesStore
{
    string? Get(string key);
    void Set(string key, string value);
}

