using System;
using System.ComponentModel.DataAnnotations;

namespace OneJaxDashboard.Models
{
    public class EventEntryViewModel
    {
        [Required(ErrorMessage = "Event Name is required.")]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event Date is required.")]
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1/1/2020", "12/31/2030", ErrorMessage = "Date must be between 01/01/2020 and 12/31/2030.")]
            public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Satisfaction score is required.")]
        [Range(1, 10, ErrorMessage = "Satisfaction must be between 1 and 10.")]
        public int PostEventSatisfaction { get; set; }

        [Required(ErrorMessage = "Pre-Assessment Resiliency score is required.")]
        [Range(1, 10, ErrorMessage = "Score must be between 1 and 10.")]
        public int PreResiliency { get; set; }

        [Required(ErrorMessage = "Pre-Assessment Communication score is required.")]
        [Range(1, 10, ErrorMessage = "Score must be between 1 and 10.")]
        public int PreCommunication { get; set; }

        [Required(ErrorMessage = "Post-Assessment Resiliency score is required.")]
        [Range(1, 10, ErrorMessage = "Score must be between 1 and 10.")]
        public int PostResiliency { get; set; }

        [Required(ErrorMessage = "Post-Assessment Communication score is required.")]
        [Range(1, 10, ErrorMessage = "Score must be between 1 and 10.")]
        public int PostCommunication { get; set; }
    }
}