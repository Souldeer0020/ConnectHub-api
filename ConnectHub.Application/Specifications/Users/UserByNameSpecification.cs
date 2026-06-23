using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Specifications.Users
{
    public class UserByNameSpecification :BaseSpecification<User>
    {
        public UserByNameSpecification(string userName)
        {
                AddCriteria(u=>u.UserName == userName);
            AddInclude(u => u.Posts);
            AddInclude(u => u.Comments);
            AddInclude(u => u.Followers);
            AddInclude(u => u.Following);
            AddInclude(u => u.Likes);
            AddInclude(u => u.Notifications);
        }
    }
}
