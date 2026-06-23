using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Interfaces
{
    public interface IUnitOfWork :IDisposable
    {
        public IGenericRepository<User> Users { get; }
        public IGenericRepository<Post> Posts { get; }
        public IGenericRepository<Comment> Comments { get; }
        public IGenericRepository<Like> Likes { get; }
        public IGenericRepository<Follow> Follows { get; }
        public IGenericRepository<Notification> Notifications { get; }
        Task<int> SaveChangesAsync();
    }
}
