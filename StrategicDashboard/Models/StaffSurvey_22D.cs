using System;
using System.ComponentModel.DataAnnotations;
//Karrie's
namespace OneJaxDashboard.Models
{
    public class StaffSurvey_22D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a staff member.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a satisfaction rate.")]
        [Range(0, 100, ErrorMessage = "Satisfaction rate must be between 0 and 100.")]
        public int SatisfactionRate { get; set; }

        [Required(ErrorMessage = "Please enter the number of professional development activities.")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of professional development activities must be a positive number.")]
        public int ProfessionalDevelopmentCount { get; set; }

    }
}
