using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Specifications.Likes
{
    public class LikeExistsSpecification :BaseSpecification<Like>
    {
        public LikeExistsSpecification(int userId,int postId)
        {
            AddCriteria(l => l.UserId == userId && l.PostId == postId);
        }
    }
}
