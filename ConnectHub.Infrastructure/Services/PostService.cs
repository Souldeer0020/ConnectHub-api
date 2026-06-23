using ConnectHub.Application.Constants;
using ConnectHub.Application.DTO_s.Posts;
using ConnectHub.Application.Interfaces;
using ConnectHub.Application.Specifications.Likes;
using ConnectHub.Application.Specifications.Posts;
using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Infrastructure.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ICacheService _cache;

        public PostService(IUnitOfWork unitOfWork,INotificationService notificationService,ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _cache = cache;
        }
        public async Task<PostResponseDto> CreatePostAsync(int userId, CreatePostDto dto)
        {
            var post = new Post()
            {
                Content = dto.Content,
                ImageUrl = dto.ImageUrl,
                UserId = userId
            };

            await _unitOfWork.Posts.AddAsync(post);
            await _unitOfWork.SaveChangesAsync();

            return await GetPostByIdAsync(post.Id);
        }

        public async Task DeletePostAsync(int postId, int currentUserId)
        {
            var spec = new PostByIdSpecification(postId);

            var post = await _unitOfWork.Posts.GetBySpecAsync(spec)
                ??throw new Exception("Post not found.");

            if (post.UserId != currentUserId)
                throw new UnauthorizedAccessException("You can only delete your own posts.");

             _unitOfWork.Posts.Delete(post);

            await _cache.RemoveAsync(CacheKeys.Post(postId));
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedResponseDto<PostResponseDto>> GetAllPostsAsync(int pageNumber, int pageSize)
        {
            var spec = new PostsWithPaginationSpecification(pageNumber, pageSize);
            var countSpec = new PostsWithPaginationSpecification(1, int.MaxValue); // to get total posts 
            
            var posts = await _unitOfWork.Posts.ListBySpecAsync(spec); // get all posts
            var totalCount = await _unitOfWork.Posts.CountBySpecAsync(countSpec); // to get total posts 

            return new PaginatedResponseDto<PostResponseDto>
            {
                Data = posts.Select(MapToDto).ToList(), // used to map from post to PostResponseDto
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        private PostResponseDto MapToDto(Post post) => // map from post to PostResponseDto
            new()
            {
                Id = post.Id,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                CreatedAt = (DateTime)post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                UserId = post.UserId,
                UserName = post.User?.UserName ?? string.Empty,
                ProfilePictureUrl = post.User?.ProfilePictureUrl,
                LikesCount = post.Likes?.Count ?? 0,
                CommentsCount = post.Comments?.Count ?? 0
            };
        

        public async Task<PostResponseDto> GetPostByIdAsync(int postId)
        {
            var cachedKey = CacheKeys.Post(postId);
            var cached = await _cache.GetAsync<PostResponseDto>(cachedKey);

            if (cached is not null)
                return cached;

            var spec = new PostByIdSpecification(postId);

            var post = await _unitOfWork.Posts.GetBySpecAsync(spec)
                ?? throw new Exception("Post not found");

            var postDto = MapToDto(post);

            await _cache.SetAsync(cachedKey, postDto, TimeSpan.FromMinutes(10));

            return postDto;
        }

        public async Task<PaginatedResponseDto<PostResponseDto>> GetPostsByUserAsync(int userId, int pageNumber, int pageSize)
        {
            var spec = new PostsByUserSpecification(userId, pageNumber, pageSize);
            var countSpec = new PostsByUserSpecification(userId, 1, int.MaxValue);

            var posts = await _unitOfWork.Posts.ListBySpecAsync(spec);
            var totalCount = await _unitOfWork.Posts.CountBySpecAsync(countSpec);

            return new PaginatedResponseDto<PostResponseDto>
            {
                Data = posts.Select(MapToDto).ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PostResponseDto> UpdatePostAsync(int postId, int currentUserId, UpdatePostDto dto)
        {
            var spec = new PostByIdSpecification(postId);
            var post = await _unitOfWork.Posts.GetBySpecAsync(spec)
                ?? throw new Exception("Post not found.");

            if (post.UserId != currentUserId)
                throw new UnauthorizedAccessException("You can only edit your own posts.");

            if (!string.IsNullOrWhiteSpace(dto.Content))
                post.Content = dto.Content;

            if (dto.ImageUrl != null)
                post.ImageUrl = dto.ImageUrl;

            post.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Posts.Update(post);
            await _unitOfWork.SaveChangesAsync();
            await _cache.RemoveAsync(CacheKeys.Post(postId));

            return MapToDto(post);
        }

        public async Task LikePostAsync(int postId, int currentUserId)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(postId) ??
                throw new Exception("Post not found");

            var existingLike = await _unitOfWork.Likes.GetBySpecAsync(new LikeExistsSpecification(currentUserId, postId));

            if (existingLike != null)
                throw new Exception("You already liked this post");

            var like = new Like
            {
                PostId = postId,
                UserId = currentUserId
            };

            await _unitOfWork.Likes.AddAsync(like);
            await _unitOfWork.SaveChangesAsync();

            if(post.UserId != currentUserId)
            {
                var liker = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                await _notificationService.CreateNotificationAsync(post.UserId, NotificationType.Like, $"{liker!.UserName} liked your post");
            }
        }

        public async Task UnlikePostAsync(int postId, int currentUserId)
        {
            var existingLike = await _unitOfWork.Likes.GetBySpecAsync(new LikeExistsSpecification(currentUserId, postId));
            if (existingLike == null)
                throw new Exception("you didnt like this video before to dislike it now");

            _unitOfWork.Likes.Delete(existingLike);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PostResponseDto> AddCommentAsync(int postId, int currentUserId, string content)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(postId) ??
                throw new Exception("Post not found");

            var comment = new Comment
            {
                PostId = postId,
                UserId = currentUserId,
                Content = content
            };

            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.SaveChangesAsync();

            if (post.UserId != currentUserId)
            {
                var commenter = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                await _notificationService.CreateNotificationAsync(post.UserId, NotificationType.Comment, $"{commenter.UserName} Commented on your post");
            }
            return await GetPostByIdAsync(postId);
        }
    }
}
