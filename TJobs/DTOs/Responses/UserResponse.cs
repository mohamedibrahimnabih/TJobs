namespace TJobs.DTOs.Responses
{
    public class UserResponse
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LockoutEnabled { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = null!;
    }
}
