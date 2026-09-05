using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // EXPENDITURES
        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Personnel Expenses")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PersonnelExpenses { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Contract & Professional Services")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ContractProfessionalServices { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Operating Expenses")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OperatingExpenses { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Program Expenses")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ProgramExpenses { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Advertising & Marketing")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AdvertisingMarketing { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Professional Development")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ProfessionalDevelopmentExpense { get; set; }

        
        // REVENUES
        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Individual Giving")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? IndividualGiving { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Corporate & Foundation Grants")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CorporateFoundationGrants { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Humanitarian Awards")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? HumanitarianAwards { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Program Revenue")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ProgramRevenue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "People & Culture Workshops")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PeopleCultureWorkshops { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Other Revenues")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OtherRevenues { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Calculated properties
        [Display(Name = "Total Expenses")]
        public decimal TotalExpenses => (PersonnelExpenses ?? 0) + (ContractProfessionalServices ?? 0) +
                                        (OperatingExpenses ?? 0) + (ProgramExpenses ?? 0) +
                                        (AdvertisingMarketing ?? 0) + (ProfessionalDevelopmentExpense ?? 0);

        [Display(Name = "Total Revenues")]
        public decimal TotalRevenues => (IndividualGiving ?? 0) + (CorporateFoundationGrants ?? 0) +
                                        (HumanitarianAwards ?? 0) + (ProgramRevenue ?? 0) +
                                        (PeopleCultureWorkshops ?? 0) + (OtherRevenues ?? 0);

        [Display(Name = "Net (Revenue - Expense)")]
        public decimal NetAmount => TotalRevenues - TotalExpenses;
    }
}
