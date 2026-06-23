using ConnectHub.Application.DTO_s.Users;
using ConnectHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConnectHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetProfile(int id)
        {
                var result = await _userService.GetUserProfileAsync(id);
                return Ok(result);
        }



        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto updateProfileDto)
        {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _userService.UpdateProfileAsync(userId, updateProfileDto);
                return Ok(result);
        }

        [HttpPost("me/avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
                if(file is null || file.Length ==0)
                    return BadRequest(new { message = "No file uploaded" });

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                using var stream = file.OpenReadStream();  // stream is used to read the file content
                var result = await _userService.UploadAvatarAsync(userId, stream,file.FileName);

                return Ok(new {ProfilePictureUrl = result });
        }
    }
}
