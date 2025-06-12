using System.ComponentModel.DataAnnotations;

namespace TJobs.DTOs.Requests
{
    public class ForgetPasswordRequest
    {
        [Required]
        public string EmailOrUserName { get; set; } = null!;
    }
}
