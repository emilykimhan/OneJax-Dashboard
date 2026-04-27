using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//Karrie's
namespace OneJaxDashboard.Models
{
    // Tracks: Increase youth attendance by at least 20%
    public class YouthAttend_15D
    {
        [Key]
        public int Id { get; set; }

        // Event pulled from the Strategies table (Name field)
        [Required(ErrorMessage = "Please select an event.")]
        [Display(Name = "Event")]
        public int StrategyId { get; set; }

        [ForeignKey("StrategyId")]
        public Strategy? Strategy { get; set; }

        [Required(ErrorMessage = "Please enter the number of youth attendees.")]
        [Range(0, 100000, ErrorMessage = "Number of youth attendees must be between 0 and 100,000.")]
        [Display(Name = "Number of Youth Attendees")]
        public int NumberOfYouthAttendees { get; set; }

        [Required(ErrorMessage = "Please enter the post-event survey satisfaction rating.")]
        [Range(0, 100, ErrorMessage = "Post-event survey satisfaction must be between 0 and 100.")]
        [Display(Name = "Post-Event Survey Satisfaction (%)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PostEventSurveySatisfaction { get; set; }

        [Required(ErrorMessage = "Please enter the average pre-assessment score.")]
        [Range(0, 100, ErrorMessage = "Average pre-assessment must be between 0 and 100.")]
        [Display(Name = "Average Pre-Assessment of Resiliency & Communication")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AveragePreAssessment { get; set; }

        [Required(ErrorMessage = "Please enter the average post-assessment score.")]
        [Range(0, 100, ErrorMessage = "Average post-assessment must be between 0 and 100.")]
        [Display(Name = "Average Post-Assessment of Resiliency & Communication")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AveragePostAssessment { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Helper: improvement between pre and post assessment
        public decimal AssessmentImprovement => AveragePostAssessment - AveragePreAssessment;
    }
}
