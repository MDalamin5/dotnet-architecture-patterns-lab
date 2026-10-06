using System;
using System.ComponentModel.DataAnnotations;
using TEcommerceWebApi.Enums;

namespace TEcommerceWebApi.DTOs
{
    public class AuthRegisterDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full Name must be between 2 and 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Customer;

        // Optional: Can be passed in JSON body if testing on localhost without headers
        public Guid? TenantId { get; set; }
    }
}