using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Specifications.Follows
{
    public class FollowersSpecification : BaseSpecification<Follow>
    {
        public FollowersSpecification(int followerId)
        {
            AddCriteria(f => f.FollowingId == followerId);
            AddInclude(f => f.Follower);
        }
    }
}
