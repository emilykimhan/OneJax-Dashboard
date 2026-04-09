using System.ComponentModel.DataAnnotations;
//Karrie's 
namespace OneJaxDashboard.Models
{
    public class selfAssess_31D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a year.")]
        [Range(2020, 2100, ErrorMessage = "Year must be between 2020 and 2100.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Please select a month.")]
        [StringLength(20, ErrorMessage = "Month must be 20 characters or fewer.")]
        public string Month { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter average annual board self-eval score.")]
        [Range(0, 100, ErrorMessage = "Self-assessment score must be between 0 and 100.")]
        public int SelfAssessmentScore { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
