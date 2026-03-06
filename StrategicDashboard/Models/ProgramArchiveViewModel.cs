namespace OneJaxDashboard.Models;

public class ProgramArchiveViewModel
{
    public List<ArchivedProgram> Programs { get; set; } = new();
    public List<Strategy> Events { get; set; } = new();
}
