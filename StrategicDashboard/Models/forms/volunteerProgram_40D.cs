using System.ComponentModel.DataAnnotations;

namespace OneJaxDashboard.Models
{
    public class volunteerProgram_40D
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a quarter.")]
        [Range(1, 4, ErrorMessage = "Quarter must be between 1 and 4.")]
        public int Quarter { get; set; } // 1 = Q1, 2 = Q2, 3 = Q3, 4 = Q4

        [Required(ErrorMessage = "Please enter a year.")]
        [Range(2020, 2100, ErrorMessage = "Year must be between 2020 and 2100.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Please enter the number of volunteers.")]
        [Range(0, 10000, ErrorMessage = "Number of volunteers must be between 0 and 10,000.")]
        public int NumberOfVolunteers { get; set; }

        public bool ProgramEstablished { get; set; } = false;

        [StringLength(2000)]
        public string CommunicationsActivities { get; set; } = string.Empty;

        [StringLength(2000)]
        public string RecognitionActivities { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Number of volunteer-led initiatives must be between 0 and 100.")]
        public int VolunteerLedInitiatives { get; set; } = 0;

        [StringLength(2000)]
        public string InitiativeDescriptions { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
