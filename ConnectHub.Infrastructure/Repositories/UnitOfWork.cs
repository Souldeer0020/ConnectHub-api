using ConnectHub.Application.Interfaces;
using ConnectHub.Domain.Entities;
using ConnectHub.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;

        public IGenericRepository<User> Users { get; }

        public IGenericRepository<Post> Posts { get; }

        public IGenericRepository<Comment> Comments { get; }

        public IGenericRepository<Like> Likes { get; }

        public IGenericRepository<Follow> Follows { get; }

        public IGenericRepository<Notification> Notifications{ get; }
        public UnitOfWork(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            Users=new GenericRepository<User>(_dbContext);
            Posts = new GenericRepository<Post>(_dbContext);
            Comments = new GenericRepository<Comment>(_dbContext);
            Likes = new GenericRepository<Like>(_dbContext);
            Follows = new GenericRepository<Follow>(_dbContext);
            Notifications = new GenericRepository<Notification>(_dbContext);
        }


        public async Task<int> SaveChangesAsync()
        {
            int result =await _dbContext.SaveChangesAsync();
            return result;
        }


        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
