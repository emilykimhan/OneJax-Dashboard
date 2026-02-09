using System.ComponentModel.DataAnnotations;
//Karrie's 
namespace OneJaxDashboard.Models
{
    public class feeForService_21D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the client name.")]
        [StringLength(200)]
        public string ClientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a program title.")]
        public int StrategyId { get; set; }

        public Strategy? Strategy { get; set; }

        [StringLength(200)]
        public string? ProgramTitle { get; set; }

        [Required(ErrorMessage = "Please enter the date.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = new DateTime(2020, 1, 1);

        [Required(ErrorMessage = "Please enter the total number of workshops.")]
        [Range(1, 1000, ErrorMessage = "Number of workshops must be at least 1.")]
        public int TotalNumberOfWorkshops { get; set; }

        [Required(ErrorMessage = "Please specify if the workshop is in person or online.")]
        [StringLength(50)]
        public string WorkshopFormat { get; set; } = string.Empty; // "In Person" or "Online"

        [StringLength(200)]
        public string? WorkshopLocation { get; set; } // Only if in person

        [Required(ErrorMessage = "Please enter the workshop date.")]
        [DataType(DataType.Date)]
        public DateTime WorkshopDate { get; set; } = new DateTime(2020, 1, 1);

        [StringLength(500)]
        public string? EventPartners { get; set; }

        [Required(ErrorMessage = "Please enter the number of attendees.")]
        [Range(1, 10000, ErrorMessage = "Number of attendees must be at least 1.")]
        public int NumberOfAttendees { get; set; }

        [Required(ErrorMessage = "Please enter the participant satisfaction rating.")]
        [Range(0, 100, ErrorMessage = "Participant satisfaction rating must be between 0 and 100.")]
        public decimal ParticipantSatisfactionRating { get; set; }

        [Required(ErrorMessage = "Please enter the partner satisfaction rating.")]
        [Range(0, 100, ErrorMessage = "Partner satisfaction rating must be between 0 and 100.")]
        public decimal PartnerSatisfactionRating { get; set; }

        [Required(ErrorMessage = "Please enter the revenue received.")]
        [Range(0, double.MaxValue, ErrorMessage = "Revenue must be a positive number.")]
        [DataType(DataType.Currency)]
        public decimal RevenueReceived { get; set; }

        [Required(ErrorMessage = "Please select a quarter.")]
        [StringLength(10)]
        public string Quarter { get; set; } = string.Empty; // "Q1", "Q2", "Q3", "Q4"

        [Required(ErrorMessage = "Please enter the year.")]
        [Range(2020, 2100, ErrorMessage = "Please enter a valid year.")]
        public int Year { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
