using Microsoft.EntityFrameworkCore;

namespace TJobs.Models
{
    public enum UserRequestStatus
    {
        Pending,
        Accepted,
        Rejected,
        Completed
    }

    public class UserRequest
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public int RequestId { get; set; }
        public Request Request { get; set; } = null!;

        public UserRequestStatus UserRequestStatus { get; set; }
        public DateTime? ApplyDateTime { get; set; }
    }
}
