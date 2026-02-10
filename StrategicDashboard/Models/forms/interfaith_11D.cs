using System.ComponentModel.DataAnnotations;

namespace OneJaxDashboard.Models
{
    public class interfaith_11D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select an event name.")]
        public int StrategyId { get; set; }

        public Strategy? Strategy { get; set; }

        [Required(ErrorMessage = "Please enter the number of faiths represented.")]
        [Range(1, 100, ErrorMessage = "Number of faiths represented must be between 1 and 100.")]
        public int NumberOfFaithsRepresented { get; set; }

        [Required(ErrorMessage = "Please enter the post-event satisfaction survey rating.")]
        [Range(0, 100, ErrorMessage = "Post-event satisfaction rating must be between 0 and 100.")]
        public int PostEventSatisfactionSurvey { get; set; }

        [Required(ErrorMessage = "Please enter the total attendance.")]
        [Range(1, 10000, ErrorMessage = "Total attendance must be between 1 and 10000.")]
        public int TotalAttendance { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
