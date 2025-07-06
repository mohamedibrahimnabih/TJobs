namespace TJobs.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty; // المستلم
        public ApplicationUser User { get; set; } = null!;

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public NotificationType Type { get; set; } = NotificationType.System;
    }

    public enum NotificationType
    {
        System,
        Message,
        Request,
        Accept,
        Reject
    }
}
