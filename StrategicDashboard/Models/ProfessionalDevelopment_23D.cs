using System.ComponentModel.DataAnnotations;
//Karrie's 
namespace OneJaxDashboard.Models
{
    public class ProfessionalDevelopment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a staff member.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the number of professional development activities for the year 2026.")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of professional development activities must be at least 1.")]
        public int ProfessionalDevelopmentYear26 { get; set; }

         [Required(ErrorMessage = "Please enter the number of professional development activities for the year 2027.")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of professional development activities must be at least 1.")]
        public int ProfessionalDevelopmentYear27 { get; set; }
    }
}