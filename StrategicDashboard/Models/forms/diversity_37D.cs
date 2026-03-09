using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//Karrie's
namespace OneJaxDashboard.Models
{
    // Tracks: Increase participant diversity by 10% compared to FY 26-27
    public class Diversity_37D
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

        // Diversity: number of diverse participants
        [Required(ErrorMessage = "Please enter the number of diverse participants.")]
        [Range(0, 100000, ErrorMessage = "Diversity count must be between 0 and 100,000.")]
        [Display(Name = "Number of Diverse Participants")]
        public int DiversityCount { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
