using System.ComponentModel.DataAnnotations;
//Karrie's 
namespace OneJaxDashboard.Models
{
    public class Plan2026_24D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Year is required")]
        [Range(2020, 2100, ErrorMessage = "Please enter a valid year")]
        [Display(Name = "Year")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Quarter is required")]
        [Display(Name = "Quarter")]
        [RegularExpression("^Q[1-4]$", ErrorMessage = "Please enter a valid quarter (Q1, Q2, Q3, or Q4)")]
        [MaxLength(2)]
        public string Quarter { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Framework Status")]
        [MaxLength(100)]
        public string FrameworkStatus { get; set; } = string.Empty;

        [Display(Name = "Notes")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        [Display(Name = "Goal Met")]
        public bool GoalMet { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
    
