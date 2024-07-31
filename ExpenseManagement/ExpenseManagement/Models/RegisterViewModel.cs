using System.ComponentModel.DataAnnotations;

namespace ExpenseManagement.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Confirmation Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [Compare("Email", ErrorMessage = "The Email and Confirmation Email do not match.")]
        public string ConfirmEmail { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirmation Password is required.")]
        [Compare("Password", ErrorMessage = "The Password and Confirmation Password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
