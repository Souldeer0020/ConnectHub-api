using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Specifications.Posts
{
    public class PostByIdSpecification : BaseSpecification<Post>
    {
        public PostByIdSpecification(int postId)
        {
            AddCriteria(p => p.Id == postId);
             AddInclude(p => p.User);
             AddInclude(p => p.Comments);
             AddInclude(p => p.Likes);
        }
    }
}
