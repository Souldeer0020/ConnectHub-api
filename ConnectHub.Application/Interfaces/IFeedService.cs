using ConnectHub.Application.DTO_s.Posts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Interfaces
{
    public interface IFeedService
    {
        Task<PaginatedResponseDto<PostResponseDto>> GetFeedAsync(int currentUserId,int pageNumber, int pageSize);
    }
}
