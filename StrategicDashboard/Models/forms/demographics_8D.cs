using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//Karrie's 
namespace OneJaxDashboard.Models
{
    public class demographics_8D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Event is required")]
        [Display(Name = "Event")]
        public int StrategyId { get; set; }

        [ForeignKey("StrategyId")]
        public Strategy? Strategy { get; set; }

        [Required(ErrorMessage = "Year is required")]
        [Range(2020, 2100, ErrorMessage = "Please enter a valid year")]
        [Display(Name = "Year")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Zip codes are required")]
        [Display(Name = "Zip Codes")]
        [MaxLength(1000)]
        public string ZipCodes { get; set; } = string.Empty;

        [Display(Name = "Notes")]
        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

