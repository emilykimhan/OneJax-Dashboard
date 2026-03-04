using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//Karrie's
namespace OneJaxDashboard.Models
{
    // Tracks: Ensure 25% of attendees are first-time participants across programs
    public class FirstTime_38D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a fiscal year.")]
        [Display(Name = "Fiscal Year")]
        public string FiscalYear { get; set; } = string.Empty;

        // Event pulled from the Strategies table (Name field)
        [Required(ErrorMessage = "Please select an event.")]
        [Display(Name = "Event")]
        public int StrategyId { get; set; }

        [ForeignKey("StrategyId")]
        public Strategy? Strategy { get; set; }

        [Required(ErrorMessage = "Please enter the total number of attendees.")]
        [Range(1, 100000, ErrorMessage = "Total attendees must be between 1 and 100,000.")]
        [Display(Name = "Total Number of Attendees")]
        public int TotalAttendees { get; set; }

        [Required(ErrorMessage = "Please enter the number of first-time participants.")]
        [Range(0, 100000, ErrorMessage = "Number of first-time participants must be between 0 and 100,000.")]
        [Display(Name = "Number of First-Time Participants")]
        public int NumberOfFirstTimeParticipants { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Helper: percentage of attendees who are first-time participants
        public decimal FirstTimeParticipantRate =>
            TotalAttendees > 0 ? Math.Round((decimal)NumberOfFirstTimeParticipants / TotalAttendees * 100, 2) : 0;

        // Helper: whether the 25% goal is met
        public bool GoalMet => FirstTimeParticipantRate >= 25;
    }
}
