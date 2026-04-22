using System.ComponentModel.DataAnnotations;
//Karrie's 
namespace OneJaxDashboard.Models
{
    public class achieveMile_6D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Year is required")]
        [Range(2000, 2100, ErrorMessage = "Please enter a valid year")]
        [Display(Name = "Year")]
        public int Year { get; set; } = DateTime.Now.Year;

        [Range(1, 12, ErrorMessage = "Please select a valid month")]
        [Display(Name = "Month")]
        public int? Month { get; set; }

        [Required(ErrorMessage = "Percentage is required")]
        [Range(0, 100, ErrorMessage = "Percentage must be between 0 and 100")]
        [Display(Name = "Percentage of Milestones Achieved")]
        public decimal Percentage { get; set; }

        [Display(Name = "Achieved in 6 Month Review")]
        public bool achievedReview { get; set; }

        [Display(Name = "Goal Met")]
        public bool GoalMet => Percentage >= 75;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}