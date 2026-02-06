using System.ComponentModel.DataAnnotations;
//Karrie's 
namespace OneJaxDashboard.Models
{
    public class Annual_average_7D
    {        
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Year is required")]
        [Range(2020, 2100, ErrorMessage = "Please enter a valid year")]
        [Display(Name = "Year")]
        public int Year { get; set; }
        [Required(ErrorMessage = "Percentage is required")]
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        [Display(Name = "Percentage of Respondents Identifying OneJax as Trusted Leader")]
        public decimal Percentage { get; set; }
 
        [Display(Name = "Total Respondents")]
        [Range(0, int.MaxValue, ErrorMessage = "Total respondents must be a positive number")]
        public int? TotalRespondents { get; set; }
        [Display(Name = "Respondents Identifying as Trusted Leader")]
        [Range(0, int.MaxValue, ErrorMessage = "Must be a positive number")]
        public int? RespondentsIdentifyingAsTrusted { get; set; }
        [Display(Name = "Notes")]
        [MaxLength(500)]
        public string? Notes { get; set; }
        [Display(Name = "Goal Met")]
        public bool GoalMet => Percentage >= 70;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}