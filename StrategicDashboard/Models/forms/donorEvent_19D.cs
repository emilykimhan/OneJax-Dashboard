using System.ComponentModel.DataAnnotations;
//Karrie's 
namespace OneJaxDashboard.Models
{
    public class DonorEvent_19D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select an event name.")]
        public int StrategyId { get; set; }

        public Strategy? Strategy { get; set; }

        [Required(ErrorMessage = "Please enter the number of participants.")]
        [Range(1, 10000, ErrorMessage = "Number of participants must be between 1 and 10000.")]
        public int NumberOfParticipants { get; set; }

        [Required(ErrorMessage = "Please enter the event satisfaction rating.")]
        [Range(0, 100, ErrorMessage = "Event satisfaction rating must be between 0 and 100.")]
        public int EventSatisfactionRating { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
