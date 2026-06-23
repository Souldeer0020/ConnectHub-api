using ConnectHub.Application.DTO_s.Posts;
using ConnectHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConnectHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost(CreatePostDto dto)
        {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _postService.CreatePostAsync(userId, dto);
                return Ok(result);
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPost(int postId)
        {
                var result = await _postService.GetPostByIdAsync(postId);
                return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _postService.GetAllPostsAsync(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPostsByUser(int userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize=10)
        {
            var result = await _postService.GetPostsByUserAsync(userId, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpPut("{postId}")]
        [Authorize]
        public async Task<IActionResult> UpdatePost(int postId,UpdatePostDto dto)
        {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!); // the "!" is a null forgiving operator which means “I know this value might look nullable, but trust me — it won't be null.”
                var result =await _postService.UpdatePostAsync(postId, currentUserId, dto);
                return Ok(result);
        }

        [HttpDelete("{postId}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int postId)
        {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                 await _postService.DeletePostAsync(postId, currentUserId);
                return NoContent();
        }

        [HttpPost("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> LikePost(int postId)
        {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _postService.LikePostAsync(postId, userId);
                return Ok(new { message = "Post is liked" });
        }

        [HttpDelete("{postId}/like")]
        [Authorize]
        public async Task<IActionResult> unlikePost(int postId)
        {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _postService.UnlikePostAsync(postId, userId);
                return Ok(new { message = "post is unliked" });
        }

        [HttpPost("{postId}/comment")]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, [FromBody] string content)
        {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result= await _postService.AddCommentAsync(postId, userId, content);
                return Ok(result);
        }
    }
}
