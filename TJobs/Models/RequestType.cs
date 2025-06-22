namespace TJobs.Models
{
    public class RequestType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<Request> Requests { get; set; } = null!;
    }
}
