using System.ComponentModel.DataAnnotations;

namespace OneJaxDashboard.Models
{
    public class BudgetTracking_28D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a quarter.")]
        [StringLength(10)]
        [Display(Name = "Quarter")]
        public string Quarter { get; set; } = string.Empty; // "Q1", "Q2", "Q3", "Q4"

        [Required(ErrorMessage = "Please enter the year.")]
        [Range(2022, 2100, ErrorMessage = "Please enter a valid year.")]
        [Display(Name = "Year")]
        public int Year { get; set; } = 2022;

        // EXPENSES
        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Community Programs")]
        public decimal? CommunityPrograms { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "OneYouth Programs")]
        public decimal? OneYouthPrograms { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Interfaith Programs")]
        public decimal? InterfaithPrograms { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Humanitarian Event")]
        public decimal? HumanitarianEvent { get; set; }

        // REVENUES
        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Corporate Giving")]
        public decimal? CorporateGiving { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Individual Giving")]
        public decimal? IndividualGiving { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Grants & Foundations")]
        public decimal? GrantsFoundations { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Community Events")]
        public decimal? CommunityEvents { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "People and Culture Workshops")]
        public decimal? PeopleCultureWorkshops { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Calculated properties
        [Display(Name = "Total Expenses")]
        public decimal TotalExpenses => (CommunityPrograms ?? 0) + (OneYouthPrograms ?? 0) + 
                                        (InterfaithPrograms ?? 0) + (HumanitarianEvent ?? 0);

        [Display(Name = "Total Revenues")]
        public decimal TotalRevenues => (CorporateGiving ?? 0) + (IndividualGiving ?? 0) + 
                                        (GrantsFoundations ?? 0) + (CommunityEvents ?? 0) + 
                                        (PeopleCultureWorkshops ?? 0);

        [Display(Name = "Net (Revenue - Expense)")]
        public decimal NetAmount => TotalRevenues - TotalExpenses;
    }
}
