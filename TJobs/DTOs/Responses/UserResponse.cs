namespace TJobs.DTOs.Responses
{
    public class UserResponse
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? SSN { get; set; }
        public List<string> Roles { get; set; } = null!;
    }
}
