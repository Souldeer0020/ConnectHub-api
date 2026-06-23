using ConnectHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConnectHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost("{UserId}")]
        [Authorize]
        public async Task<IActionResult> Follow(int UserId)
        {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var targetUserName = await _followService.FollowAsync(currentUserId, UserId);
                return Ok($"Successfully followed the user {targetUserName}");
        }

        [HttpDelete("{UserId}")]
        [Authorize]
        public async Task<IActionResult> Unfollow(int UserId)
        {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var targetUserName = await _followService.UnfollowAsync(currentUserId, UserId);
                return Ok($"Successfully unfollowed user {targetUserName}");
        }
        [HttpGet("{UserId}/followers")]
        public async Task<IActionResult> GetFollowers(int UserId)
        {
                var followers = await _followService.GetFollowersAsync(UserId);
                return Ok(followers);
        }

        [HttpGet("{UserId}/following")]
        public async Task<IActionResult> GetFollowing(int UserId)
        {
                var following = await _followService.GetFollowingAsync(UserId);
                return Ok(following);
        }
        [HttpGet("{UserId}/stats")]
        public async Task<IActionResult> GetFollowStats(int UserId)
        {
                var CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _followService.GetFollowStatsAsync(CurrentUserId, UserId);
                return Ok(result);
        }
    }
}
