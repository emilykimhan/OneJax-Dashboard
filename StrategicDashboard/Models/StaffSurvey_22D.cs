using System;
using System.ComponentModel.DataAnnotations;
//Karrie's
namespace OneJaxDashboard.Models
{
    public class StaffSurvey_22D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a year.")]
        [Range(2020, 2030, ErrorMessage = "Year must be between 2020 and 2030.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Please select a month.")]
        [StringLength(20, ErrorMessage = "Month must be 20 characters or fewer.")]
        public string Month { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a satisfaction rate.")]
        [Range(0, 100, ErrorMessage = "Satisfaction rate must be between 0 and 100.")]
        public int SatisfactionRate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

    }
}
