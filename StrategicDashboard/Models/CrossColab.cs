using System.ComponentModel.DataAnnotations;

namespace OneJaxDashboard.Models;

public class CrossColab
{
    public int Id { get; set; }

    public int StrategyId { get; set; }

    [Required]
    [MaxLength(200)]
    public string PartnerName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string? PartnerEmail { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public Strategy? Strategy { get; set; }
}
