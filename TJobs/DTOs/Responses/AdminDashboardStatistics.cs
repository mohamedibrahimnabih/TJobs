namespace TJobs.DTOs.Responses
{
    public class AdminDashboardStatistics
    {
        public int TotalNumberOfUsers { get; set; }
        public int TotalNumberOfRequests { get; set; }
        public int TotalNumberOfBlockedUsers { get; set; }
        public int TotalNumberOfApplications { get; set; }
        public List<ChartItem> UserTypeChart { get; set; } = new();
        public List<ChartItem> GenderChart { get; set; } = new();
    }

    public class ChartItem
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
