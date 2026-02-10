using System;
using System.ComponentModel.DataAnnotations;

namespace OneJaxDashboard.Models
{
    public class BoardMeetingAttendance
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a board meeting date.")]
        [Display(Name = "Board Meeting Date")]
        public DateTime MeetingDate { get; set; } = new DateTime(2023, 1, 1);

        [Required(ErrorMessage = "Please enter the number of board members in attendance.")]
        [Range(0, int.MaxValue, ErrorMessage = "Number of board members must be a positive number.")]
        [Display(Name = "Number of Board Members in Attendance")]
        public int MembersInAttendance { get; set; }

        [Display(Name = "Total Board Members")]
        public int? TotalBoardMembers { get; set; }

        [Display(Name = "Attendance Rate (%)")]
        public decimal? AttendanceRate 
        { 
            get 
            {
                if (TotalBoardMembers.HasValue && TotalBoardMembers.Value > 0)
                {
                    return Math.Round((decimal)MembersInAttendance / TotalBoardMembers.Value * 100, 2);
                }
                return null;
            }
        }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
