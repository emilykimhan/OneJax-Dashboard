using System.ComponentModel.DataAnnotations;
//Karrie's
namespace OneJaxDashboard.Models
{
    public class EngagementEvent_5D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the year.")]
        [Range(2020, 2100, ErrorMessage = "Please enter a valid year.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Please enter the number of attendees.")]
        [Range(0, int.MaxValue, ErrorMessage = "Number of attendees cannot be negative.")]
        public int NumberOfAttendees { get; set; }

        [Required(ErrorMessage = "Please select an event.")]
        public int EventId { get; set; } // Foreign key to Event table

        [Required(ErrorMessage = "Please enter the baseline value.")]
        [Range(0, int.MaxValue, ErrorMessage = "Baseline value cannot be negative.")]
        public int BaselineValue { get; set; } = 1000; // Default baseline of 1000
        //change this later to reflect actual baseline

        [Required(ErrorMessage = "Please enter the target percent.")]
        [Range(0, 100, ErrorMessage = "Target percent must be between 0 and 100.")]
        public decimal TargetPercent { get; set; } = 10M; // Default 10% increase

        // Calculated property for target value
        public int TargetValue => BaselineValue + (int)Math.Ceiling(BaselineValue * (TargetPercent / 100));

        [Required(ErrorMessage = "Please select if target was met.")]
        public bool TargetMet { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Helper property for display
        public string Status => TargetMet ? "Met" : "Not Met";
    }
}