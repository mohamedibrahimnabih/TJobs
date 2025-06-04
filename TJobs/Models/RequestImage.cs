namespace TJobs.Models
{
    public class RequestImage
    {
        public int Id { get; set; }
        public string Img { get; set; } = string.Empty;
        public int RequestId { get; set; }

        public Request Request { get; set; } = null!;
    }
}
