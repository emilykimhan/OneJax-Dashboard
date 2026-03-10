using System.ComponentModel.DataAnnotations;

namespace OneJaxDashboard.Models
{
    public class income_27D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter the income source.")]
        [StringLength(200)]
        [Display(Name = "Income Source")]
        public string IncomeSource { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the amount.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Please select the month.")]
        [StringLength(20)]
        [Display(Name = "Month")]
        public string Month { get; set; } = string.Empty; 

        [Range(2020, 2100, ErrorMessage = "Year must be between 2020 and 2100.")]
        [Display(Name = "Year")]
        public int? Year { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
