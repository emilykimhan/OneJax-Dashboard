using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OneJaxDashboard.Models;
//Emily's
namespace OneJaxDashboard.Models
{
    public class Event
    {
        public int Id { get; set; }

        // Reference to the Strategy (Core Strategy event) this is based on
        [Display(Name = "Event")]
        public int? StrategyTemplateId { get; set; } // Nullable for events not based on strategy templates

        // The title comes from the Strategy, stored here for convenience
        public string Title { get; set; } = string.Empty;

        public string Type { get; set; } = ""; // Workshop, Meeting, Training, etc.
        public string Location { get; set; } = "";

        public decimal? SatisfactionScore { get; set; }

        public int Attendees { get; set; }

        public string Notes { get; set; } = "";

        public string PreAssessmentData { get; set; } = "";
        public string PostAssessmentData { get; set; } = "";

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

        // Link to Strategic Goal and Strategy
        [Display(Name = "Strategic Goal")]
        public int? StrategicGoalId { get; set; }

        [Display(Name = "Strategy")]
        public int? StrategyId { get; set; }

        // Ownership by username to align with current auth model
        public string OwnerUsername { get; set; } = string.Empty;

        [ForeignKey("OwnerUsername")]
        public virtual Staffauth? AssignedStaff { get; set; }

        // Admin assignment fields
        [Display(Name = "Assigned By Admin")]
        public bool IsAssignedByAdmin { get; set; } = false;

        [Display(Name = "Admin Notes")]
        public string AdminNotes { get; set; } = string.Empty;

        [Display(Name = "Assignment Date")]
        public DateTime? AssignmentDate { get; set; }

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        // Archive field to separate active events from completed/archived ones
        [Display(Name = "Archived")]
        public bool IsArchived { get; set; } = false;

        [Display(Name = "Completion Date")]
        public DateTime? CompletionDate { get; set; }

        // Navigation properties (optional for display purposes)
        [ForeignKey("StrategicGoalId")]
        public virtual StrategicGoal? StrategicGoal { get; set; }
        
        [ForeignKey("StrategyId")]
        public virtual Strategy? Strategy { get; set; }
        
        [ForeignKey("StrategyTemplateId")]
        public virtual Strategy? StrategyTemplate { get; set; }
    }
}