using System.ComponentModel.DataAnnotations;

namespace TJobs.DTOs.Requests
{
    public class ResendEmailRequest
    {
        [Required]
        public string EmailOrUserName { get; set; } = null!;
    }
}
