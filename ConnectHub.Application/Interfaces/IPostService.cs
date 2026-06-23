using ConnectHub.Application.DTO_s.Posts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Interfaces
{
    public interface IPostService
    {
        Task<PostResponseDto> CreatePostAsync(int userId, CreatePostDto dto);
        Task<PostResponseDto> GetPostByIdAsync(int postId);
        Task<PaginatedResponseDto<PostResponseDto>> GetAllPostsAsync(int pageNumber,int pageSize);
        Task<PaginatedResponseDto<PostResponseDto>> GetPostsByUserAsync(int userId, int pageNumber, int pageSize);
        Task<PostResponseDto> UpdatePostAsync(int postId, int currentUserId, UpdatePostDto dto);
        Task DeletePostAsync(int postId,int currentUserId);
        Task LikePostAsync(int postId, int currentUserId);
        Task UnlikePostAsync(int postId, int currentUserId);
        Task<PostResponseDto> AddCommentAsync(int postId, int currentUserId, string content);
    }
}
