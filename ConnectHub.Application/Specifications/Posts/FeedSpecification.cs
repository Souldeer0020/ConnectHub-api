using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Specifications.Posts
{
    public class FeedSpecification :BaseSpecification<Post>
    {
        public FeedSpecification(List<int> followingIds,int pageNumber, int pageSize)
        {
            AddCriteria(p => followingIds.Contains(p.UserId));
            AddInclude(p => p.User);
            AddInclude(p => p.Likes);
            AddInclude(p => p.Comments);
            AddOrderByDescending(p => p.CreatedAt);
            ApplyPagination(pageNumber, pageSize);
        }
    }
}
