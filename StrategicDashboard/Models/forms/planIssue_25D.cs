using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//Karrie's 
namespace OneJaxDashboard.Models
{
    public class planIssue_25D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Plan is required")]
        [Display(Name = "Framework Plan")]
        public int PlanId { get; set; }

        [ForeignKey("PlanId")]
        public Plan2026_24D? Plan { get; set; }

        [Required(ErrorMessage = "Issue name is required")]
        [Display(Name = "Issue Name")]
        [MaxLength(200)]
        public string IssueName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Crisis description is required")]
        [Display(Name = "Crisis Description")]
        [MaxLength(1000)]
        public string CrisisDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required")]
        [Range(2020, 2100, ErrorMessage = "Please enter a valid year")]
        [Display(Name = "Year")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Compliance status is required")]
        [Display(Name = "Compliant with Framework")]
        public bool IsCompliant { get; set; }

        [Display(Name = "Notes")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
