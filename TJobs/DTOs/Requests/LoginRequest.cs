using System.ComponentModel.DataAnnotations;

namespace TJobs.DTOs.Requests
{
    public class LoginRequest
    {
        [Required]
        public string EmailOrUserName { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}
