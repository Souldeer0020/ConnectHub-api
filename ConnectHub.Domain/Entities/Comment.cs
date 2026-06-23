namespace ConnectHub.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreing keys
        public int UserId { get; set; }
        public int PostId { get; set; }

        // Navigational properties
        public User User { get; set; }
        public Post Post { get; set; }
    }
}