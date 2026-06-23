namespace ConnectHub.Domain.Entities
{
    public class Like
    {
        public int UserId { get; set; }
        public int PostId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Navigational properties
        public User User { get; set; }
        public Post Post { get; set; }
    }
}