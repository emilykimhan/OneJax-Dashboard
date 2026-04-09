using System.ComponentModel.DataAnnotations;
//Karrie's 
namespace OneJaxDashboard.Models
{
    public class ProfessionalDevelopment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a year.")]
        [Range(2020, 2100, ErrorMessage = "Year must be between 2020 and 2100.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Please select a staff member.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a month.")]
        [StringLength(20, ErrorMessage = "Month must be 20 characters or fewer.")]
        public string Month { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Activities { get; set; } = string.Empty; // List of professional development activities

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
