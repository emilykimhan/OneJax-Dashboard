using System.ComponentModel.DataAnnotations;
//Karrie's
namespace OneJaxDashboard.Models
{
    public class MediaPlacements_3D
    {
        [Key]
        public int Id { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? January { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? February { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? March { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? April { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? May { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? June { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? July { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? August { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? September { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? October { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? November { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Value cannot be negative")]
        public int? December { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Calculated property for total mentions
        public int TotalMentions => (January ?? 0) + (February ?? 0) + (March ?? 0) + (April ?? 0) + 
                                    (May ?? 0) + (June ?? 0) + (July ?? 0) + (August ?? 0) + 
                                    (September ?? 0) + (October ?? 0) + (November ?? 0) + (December ?? 0);
    }
}