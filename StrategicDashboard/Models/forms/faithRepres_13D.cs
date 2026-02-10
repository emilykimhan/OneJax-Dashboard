using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OneJaxDashboard.Models
{
    public class FaithRepres_13D
    {
        [Key]
        public int Id { get; set; }

        // Reference to the Strategy (event) from strategies table
        [Display(Name = "Event")]
        [Required(ErrorMessage = "Event is required")]
        public int StrategyId { get; set; }

        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Display(Name = "Number of Faiths Represented")]
        [Range(0, int.MaxValue, ErrorMessage = "Number of faiths cannot be negative")]
        [Required(ErrorMessage = "Number of faiths is required")]
        public int NumberOfFaiths { get; set; }

        [Display(Name = "Faith Names")]
        [Required(ErrorMessage = "Faith names are required")]
        [MaxLength(500)]
        public string FaithNames { get; set; } = string.Empty; // Comma-separated list of faith names

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("StrategyId")]
        public Strategy? Strategy { get; set; }
    }
}
