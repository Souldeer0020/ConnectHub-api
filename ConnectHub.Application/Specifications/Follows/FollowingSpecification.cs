using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Specifications.Follows
{
    public class FollowingSpecification : BaseSpecification<Follow>
    {
        public FollowingSpecification(int userId)
        {
            AddCriteria(f => f.FollowerId == userId);
            AddInclude(f => f.Following);
        }
    }
}
