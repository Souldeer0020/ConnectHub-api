namespace ConnectHub.Domain.Entities
{
    public enum NotificationType
    {
        Like,
        Comment,
        Follow
    }
    public class Notification
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Foreing keys
        public int UserId { get; set; }

        //Navigational properties
        public User User { get; set; }
    }

}