namespace ConnectHub.Domain.Entities
{
    public class Follow
    {
        public int FollowerId { get; set; }
        public int FollowingId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Navigational properties
        public User Follower { get; set; }
        public User Following { get; set; }
    }
}