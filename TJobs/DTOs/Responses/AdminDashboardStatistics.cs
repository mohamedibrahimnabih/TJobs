namespace TJobs.DTOs.Responses
{
    public class AdminDashboardStatistics
    {
        public int TotalNumberOfUsers { get; set; }
        public int TotalNumberOfRequests { get; set; }
        public int TotalNumberOfBlockedUsers { get; set; }
        public int TotalNumberOfApplications { get; set; }
    }
}
