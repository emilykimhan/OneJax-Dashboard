using System.ComponentModel.DataAnnotations;

namespace StrategicDashboard.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [Display(Name = "Project Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Planned"; // Planned, In Progress, Completed

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        // Ownership by username to align with current auth model
        public string OwnerUsername { get; set; } = string.Empty;
    }
}
