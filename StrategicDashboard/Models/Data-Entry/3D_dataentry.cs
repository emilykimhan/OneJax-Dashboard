using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VMS.Models
{
    [Table("MediaPlacements")]
    public class ThreeDDataEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Placements must be 0 or higher.")]
        public int CurrentPlacements { get; set; } = 3; // initial value

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Goal must be at least 1.")]
        public int GoalPlacements { get; set; } = 12; // goal by Dec 2026

        [DataType(DataType.DateTime)]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Computed (not mapped to DB)
        [NotMapped]
        public double ProgressPercentage
        {
            get
            {
                if (GoalPlacements == 0) return 0;
                return Math.Round((double)CurrentPlacements / GoalPlacements * 100, 2);
            }
        }

        public void UpdatePlacements(int newCount)
        {
            CurrentPlacements = newCount;
            LastUpdated = DateTime.Now;
        }
    }
}