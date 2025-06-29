namespace TJobs.DTOs.Responses
{
    public class UserSkillsResponse
    {
        public List<string> Skills { get; set; } = new();
        public string Description { get; set; } = string.Empty;
    }
}
