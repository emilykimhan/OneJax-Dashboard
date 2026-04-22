using System.ComponentModel.DataAnnotations;
//Karrie's
namespace OneJaxDashboard.Models
{
    public class WebsiteTraffic_4D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Year is required")]
        [Range(2000, 2100, ErrorMessage = "Please enter a valid year")]
        public int Year { get; set; } = DateTime.Now.Year;

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? Q1_JulySeptember { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? Q2_OctoberDecember { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? Q3_JanuaryMarch { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? Q4_AprilJune { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Calculated property for total clicks
        public int TotalClicks => (Q1_JulySeptember ?? 0) + (Q2_OctoberDecember ?? 0) + 
                                  (Q3_JanuaryMarch ?? 0) + (Q4_AprilJune ?? 0);
    }
}