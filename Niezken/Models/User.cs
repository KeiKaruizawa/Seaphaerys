using System.ComponentModel.DataAnnotations;

namespace Niezken.Models
{
    public class User
    {
        public int Id { get; set; } // Primary Key (VERY IMPORTANT)

        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }
    }
}
