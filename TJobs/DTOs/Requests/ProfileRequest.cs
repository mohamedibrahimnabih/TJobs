namespace TJobs.DTOs.Requests
{
    public class ProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? SSN { get; set; }
        public List<string> SkillsOrInterests { get; set; } = new();
        public string? Description { get; set; }
    }
}
