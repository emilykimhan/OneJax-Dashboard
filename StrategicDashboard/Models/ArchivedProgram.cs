using System.ComponentModel.DataAnnotations;

namespace OneJaxDashboard.Models;

public class ArchivedProgram
{
    public int Id { get; set; }

    public int OriginalProgramId { get; set; }

    [Required]
    public string ProgramName { get; set; } = string.Empty;

    [Required]
    public string ProgramType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime ArchivedAtUtc { get; set; } = DateTime.UtcNow;
}
