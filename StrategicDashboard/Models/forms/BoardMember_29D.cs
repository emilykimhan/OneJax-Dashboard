using System.ComponentModel.DataAnnotations;

namespace OneJaxDashboard.Models
{
    public class BoardMemberRecruitment
    {
        public int Id { get; set; }

        [StringLength(1000)]
        public string MemberNames { get; set; } = string.Empty; // Comma-separated list of member names

        [Required]
        [Range(1, 4)]
        public int Quarter { get; set; } // 1 = Q1, 2 = Q2, 3 = Q3, 4 = Q4

        [Required]
        public int Year { get; set; }

        [Required]
        [Range(0, 100)]
        public int NumberRecruited { get; set; } // Total number of board members recruited in this period


    }
}
