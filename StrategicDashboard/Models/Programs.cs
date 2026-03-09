using System.ComponentModel.DataAnnotations;
//Dina's Program's Model 
namespace OneJaxDashboard.Models;

public class Programs
{
    public int Id { get; set; }

    [Required]
    public string ProgramName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    public string ProgramType { get; set; } = string.Empty;

    public List<Strategy> Strategies { get; set; } = new();
}
