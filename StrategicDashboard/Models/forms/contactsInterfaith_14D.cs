using System.ComponentModel.DataAnnotations;
//Karrie's
namespace OneJaxDashboard.Models
{
    // Tracks: Expand clergy and interfaith network contacts by 25% by end of FY 25-26
    public class ContactsInterfaith_14D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a year.")]
        [Range(2020, 2100, ErrorMessage = "Please enter a valid year.")]
        [Display(Name = "Year")]
        public int Year { get; set; }

        [Range(1, 12, ErrorMessage = "Please select a valid month.")]
        [Display(Name = "Month")]
        public int? Month { get; set; }

        [Required(ErrorMessage = "Please enter the total number of interfaith contacts.")]
        [Range(0, 100000, ErrorMessage = "Total interfaith contacts must be between 0 and 100,000.")]
        [Display(Name = "Total Interfaith Contacts")]
        public int TotalInterfaithContacts { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
