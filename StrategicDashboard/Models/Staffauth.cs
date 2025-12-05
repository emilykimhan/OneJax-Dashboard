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
        [Display(Name = "Username")]
        public string? Username { get; set; }

        [Display(Name = "Password")]
        public string? Password { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

    }
}