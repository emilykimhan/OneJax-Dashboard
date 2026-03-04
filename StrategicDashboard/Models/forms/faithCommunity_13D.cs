using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//Karrie's
namespace OneJaxDashboard.Models
{
    // Tracks: Ensure 3 faiths represented at 80% of community events
    public class FaithCommunity_13D
    {
        [Key]
        public int Id { get; set; }

        // Event pulled from the Strategies table (Name field)
        [Required(ErrorMessage = "Please select an event.")]
        [Display(Name = "Event Name")]
        public int StrategyId { get; set; }

        [ForeignKey("StrategyId")]
        public Strategy? Strategy { get; set; }

        [Required(ErrorMessage = "Please enter the number of faiths represented.")]
        [Range(1, 100, ErrorMessage = "Number of faiths represented must be between 1 and 100.")]
        [Display(Name = "Number of Faiths Represented")]
        public int NumberOfFaithsRepresented { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
