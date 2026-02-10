using System.ComponentModel.DataAnnotations;

namespace OneJaxDashboard.Models
{
    public class eventSatisfaction
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select an event name.")]
        public int StrategyId { get; set; }

        public Strategy? Strategy { get; set; }

        [Required(ErrorMessage = "Please enter the event attendee satisfaction percentage.")]
        [Range(0, 100, ErrorMessage = "Satisfaction percentage must be between 0 and 100.")]
        public decimal EventAttendeeSatisfactionPercentage { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
