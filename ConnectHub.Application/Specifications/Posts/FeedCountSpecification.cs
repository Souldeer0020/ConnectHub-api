using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Specifications.Posts
{
    public class FeedCountSpecification : BaseSpecification<Post>
    {
        public FeedCountSpecification(List<int> followingIds)
        {
            AddCriteria(p=> followingIds.Contains(p.UserId));
        }
    }
}
