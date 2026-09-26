using System.ComponentModel.DataAnnotations;
using TEcommerceWebApi.Enums;

namespace TEcommerceWebApi.DTOs
{
    public class AuthRegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        // Optional: default to Customer if not specified
        public UserRole Role { get; set; } = UserRole.Customer;
    }
}