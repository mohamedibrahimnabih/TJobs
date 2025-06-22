using System.ComponentModel.DataAnnotations;

namespace TJobs.DTOs.Requests
{
    public class RegisterRequest
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;

        [Required]
        public string UserName { get; set; } = null!;
        [Required]
        public string FirstName { get; set; } = null!;
        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = null!;

        public string? Address { get; set; }

        [Required]
        public int Age { get; set; }
        public string? SSN { get; set; }
        public UserType UserType { get; set; }

    }
}
