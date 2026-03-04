using System.ComponentModel.DataAnnotations;
//Talijah's

namespace OneJaxDashboard.Models
{
    public class Staffauth
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        // Authentication fields added for login functionality
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public required string Username { get; set; }

        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public required string Email { get; set; }

    }
}
