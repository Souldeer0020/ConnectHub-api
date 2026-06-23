using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Specifications.Posts
{
    public class PostsWithPaginationSpecification:BaseSpecification<Post>
    {
        public PostsWithPaginationSpecification(int pageNumber, int pageSize)
        {
            AddInclude(p => p.User);
            AddInclude(p => p.Comments);
            AddInclude(p => p.Likes);
            AddOrderByDescending(p => p.CreatedAt);
            ApplyPagination(pageNumber, pageSize);
        }
    }
}
