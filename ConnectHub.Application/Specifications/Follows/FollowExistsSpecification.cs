using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Specifications.Follows
{
    public class FollowExistsSpecification : BaseSpecification<Follow>
    {
        public FollowExistsSpecification(int followerId, int followingId)
        {
            AddCriteria(f => f.FollowerId == followerId && f.FollowingId == followingId);
            AddInclude(f=>f.Following);
        }
    }
}
