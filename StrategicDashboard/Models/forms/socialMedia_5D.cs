using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OneJaxDashboard.Models
{
    public class socialMedia_5D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Year is required")]
        [Range(2020, 2100, ErrorMessage = "Please enter a valid year")]
        [Display(Name = "Year")]
        public int Year { get; set; }

        [Range(0, 100, ErrorMessage = "Engagement rate must be between 0 and 100")]
        [Display(Name = "July-Sept Engagement Rate (%)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? JulySeptEngagementRate { get; set; }

        [Range(0, 100, ErrorMessage = "Engagement rate must be between 0 and 100")]
        [Display(Name = "Oct-Dec Engagement Rate (%)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OctDecEngagementRate { get; set; }

        [Range(0, 100, ErrorMessage = "Engagement rate must be between 0 and 100")]
        [Display(Name = "Jan-Mar Engagement Rate (%)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? JanMarEngagementRate { get; set; }

        [Range(0, 100, ErrorMessage = "Engagement rate must be between 0 and 100")]
        [Display(Name = "April-June Engagement Rate (%)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AprilJuneEngagementRate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Calculated property - Average engagement rate across all quarters
        [Display(Name = "Average Engagement Rate")]
        public decimal AverageEngagementRate
        {
            get
            {
                var rates = new[] { JulySeptEngagementRate, OctDecEngagementRate, JanMarEngagementRate, AprilJuneEngagementRate }
                    .Where(r => r.HasValue)
                    .Select(r => r!.Value)
                    .ToList();
                
                return rates.Any() ? rates.Average() : 0;
            }
        }

        // Goal: 30% increase - this would be calculated based on baseline
        [Display(Name = "Goal Met (30% Increase)")]
        public bool GoalMet { get; set; }
    }
}
