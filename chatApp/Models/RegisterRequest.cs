using System.ComponentModel.DataAnnotations;

namespace chatApp.Models
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Full Name must be between 3 and 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 50 characters")]
        public string Password { get; set; } = string.Empty;
    }
}
