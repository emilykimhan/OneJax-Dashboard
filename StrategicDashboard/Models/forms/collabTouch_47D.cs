using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//Karrie's
namespace OneJaxDashboard.Models
{
    // Tracks: Add 3 new collaborative partner/allied organizations annually
    public class CollabTouch_47D
    {
        [Key]
        public int Id { get; set; }

        // Fiscal year (e.g., "FY2026")
        [Required(ErrorMessage = "Please enter the fiscal year.")]
        [StringLength(20, ErrorMessage = "Fiscal year cannot exceed 20 characters.")]
        [Display(Name = "Fiscal Year")]
        public string FiscalYear { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the partner or allied organization name.")]
        [StringLength(200, ErrorMessage = "Organization name cannot exceed 200 characters.")]
        [Display(Name = "Partner / Allied Organization")]
        public string PartnerOrganization { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the contact name.")]
        [StringLength(150, ErrorMessage = "Contact name cannot exceed 150 characters.")]
        [Display(Name = "Contact")]
        public string Contact { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Contact email cannot exceed 150 characters.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Contact Email")]
        public string? ContactEmail { get; set; }

        [StringLength(30, ErrorMessage = "Contact phone cannot exceed 30 characters.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        // Event pulled from the Strategies table (Name field)
        [Required(ErrorMessage = "Please select an event.")]
        [Display(Name = "Event")]
        public int StrategyId { get; set; }

        [ForeignKey("StrategyId")]
        public Strategy? Strategy { get; set; }

        // Touchpoint: nature/type of the touchpoint with this partner
        [Required(ErrorMessage = "Please describe the touchpoint.")]
        [StringLength(500, ErrorMessage = "Touchpoint cannot exceed 500 characters.")]
        [Display(Name = "Touchpoint")]
        public string Touchpoint { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Helper: display label for the entry
        public string DisplayLabel => $"{FiscalYear} – {PartnerOrganization}";
    }
}
