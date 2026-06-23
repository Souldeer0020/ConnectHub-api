using ConnectHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Infrastructure.Persistence
{
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
            
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts {  get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set;  }

        public DbSet<Notification> Notifications {  get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Like>().HasKey(l => new { l.PostId, l.UserId });

            modelBuilder.Entity<Follow>().HasKey(f => new { f.FollowingId, f.FollowerId });

            modelBuilder.Entity<Follow>().
                HasOne(f => f.Follower).WithMany(f => f.Following).
                HasForeignKey(f => f.FollowerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Follow>().
                HasOne(f=>f.Following).WithMany(f=>f.Followers).
                HasForeignKey(f=>f.FollowingId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>().
                HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Comment>().
                HasOne(c => c.User).WithMany(c => c.Comments).
                HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Like>().
                HasOne(c => c.User).WithMany(c => c.Likes).
                HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>().
                HasIndex(u => u.UserName).IsUnique();
        }
    }
}
