using ConnectHub.Application.Constants;
using ConnectHub.Application.DTO_s.Posts;
using ConnectHub.Application.Interfaces;
using ConnectHub.Application.Specifications.Follows;
using ConnectHub.Application.Specifications.Posts;
using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Infrastructure.Services
{
    public class FeedService : IFeedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cache;

        public FeedService(IUnitOfWork unitOfWork,ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }
        public async Task<PaginatedResponseDto<PostResponseDto>> GetFeedAsync(int currentUserId, int pageNumber, int pageSize)
        {
            
            var cachedKey = CacheKeys.Feed(currentUserId, pageNumber, pageSize);
            var cached = await _cache.GetAsync<PaginatedResponseDto<PostResponseDto>>(cachedKey);

            if (cached is not null)
                return cached;

            // get all ids that the cureent user follows
            var followingsId = new FollowingSpecification(currentUserId);
            var followings = await _unitOfWork.Follows.ListBySpecAsync(followingsId);
            var followingIds = followings.Select(f => f.FollowingId).ToList();

            if (!followingIds.Any())
            {
                return new PaginatedResponseDto<PostResponseDto>
                {
                    Data = new List<PostResponseDto>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                };
            }
            var feedSpec = new FeedSpecification(followingIds, pageNumber, pageSize);
            var countspec = new FeedCountSpecification(followingIds);

            var posts = await _unitOfWork.Posts.ListBySpecAsync(feedSpec);
            var totalcount = await _unitOfWork.Posts.CountBySpecAsync(countspec);

            var result = new PaginatedResponseDto<PostResponseDto>
            {
                Data = posts.Select(MapToDto).ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalcount
            };

            await _cache.SetAsync(cachedKey, result,TimeSpan.FromMinutes(2));
            return result;
        }
        private static PostResponseDto MapToDto(Post post) =>
            new()
            {
                Id = post.Id,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                UserId = post.UserId,
                UserName= post.User?.UserName?? string.Empty,
                ProfilePictureUrl = post.User?.ProfilePictureUrl,
                LikesCount = post.Likes?.Count() ?? 0,
                CommentsCount = post.Comments?.Count() ?? 0
            };
    }
}
