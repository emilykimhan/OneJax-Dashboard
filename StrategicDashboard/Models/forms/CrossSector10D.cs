using System.ComponentModel.DataAnnotations;
//Karrie's 
namespace OneJaxDashboard.Models
{
    public class CrossSector10D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter collaboration name")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Please enter the year.")]
        [Range(2020, 2100, ErrorMessage = "Please enter a valid year.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Please enter the partner satisfaction ratings.")]
        [Range(0, 100, ErrorMessage = "Rating must be between 0 and 100.")]
        public decimal partner_satisfaction_ratings { get; set; }

        [Required(ErrorMessage = "Please select the collaboration status.")]
        public string Status { get; set; } = "Active";

        public string? Notes { get; set; } = "Notes: ";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}