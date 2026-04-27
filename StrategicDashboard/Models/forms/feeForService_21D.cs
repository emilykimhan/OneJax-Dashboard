using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        [Required(ErrorMessage = "Please select an event.")]
        public int StrategyId { get; set; }

        public Strategy? Strategy { get; set; }

        [StringLength(200)]
        public string? EventName { get; set; }

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
        [Column(TypeName = "decimal(18,2)")]
        public decimal ParticipantSatisfactionRating { get; set; }

        [Required(ErrorMessage = "Please enter the partner satisfaction rating.")]
        [Range(0, 100, ErrorMessage = "Partner satisfaction rating must be between 0 and 100.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PartnerSatisfactionRating { get; set; }

        [Required(ErrorMessage = "Please enter the revenue received.")]
        [Range(0, double.MaxValue, ErrorMessage = "Revenue must be a positive number.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RevenueReceived { get; set; }

        [Required(ErrorMessage = "Please enter the expense received.")]
        [Range(0, double.MaxValue, ErrorMessage = "Expense must be a positive number.")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpenseReceived { get; set; }

        [Required(ErrorMessage = "Please enter the year.")]
        [Range(2020, 2100, ErrorMessage = "Please enter a valid year.")]
        public int Year { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
