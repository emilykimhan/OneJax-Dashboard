using System.ComponentModel.DataAnnotations;
//Karrie's 
namespace OneJaxDashboard.Models
{
    public class Comm_rate20D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the year.")]
        [Range(2020, 2100, ErrorMessage = "Please enter a valid year.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Please enter the average communication satisfaction.")]
        [Range(0, 100, ErrorMessage = "Average communication satisfaction must be between 0 and 100.")]
        public decimal AverageCommunicationSatisfaction { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}