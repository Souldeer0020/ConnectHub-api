using ConnectHub.Application.Constants;
using ConnectHub.Application.DTO_s.Users;
using ConnectHub.Application.Interfaces;
using ConnectHub.Application.Specifications.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConnectHub.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IFileService _fileService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;

        public UserService(IFileService fileService,IUnitOfWork unitOfWork,ICacheService cacheService)
        {
            _fileService = fileService;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
        }
        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            var cacheKey = CacheKeys.UserProfile(userId);
            var cached = await _cacheService.GetAsync<UserProfileDto>(cacheKey);
            if(cached is not null)
                return cached;

            var user = await _unitOfWork.Users.GetByIdAsync(userId)
                ?? throw new Exception("User not found");


            var profile = new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                PostsCount = user.Posts.Count,
                FollowersCount = user.Followers.Count,
                FollowingCount = user.Following.Count,
                CreatedAt = user.CreatedAt
            };

            await _cacheService.SetAsync(cacheKey, profile, TimeSpan.FromMinutes(10));

            return profile;
        }

        public async Task<UserProfileDto> UpdateProfileAsync(int CurrentUserId, UpdateProfileDto dto)
        {
            var user =await _unitOfWork.Users.GetByIdAsync(CurrentUserId)
                ?? throw new Exception("User not found");


            if(!string.IsNullOrEmpty(dto.UserName)&& dto.UserName!=user.UserName)
            {
                if (_unitOfWork.Users.UserNameExists(dto.UserName))
                    throw new Exception("Username already exists");

                user.UserName = dto.UserName;
            }
            if(!string.IsNullOrEmpty(dto.Bio))
            {
                user.Bio = dto.Bio;
            }
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            await _cacheService.RemoveAsync(CacheKeys.UserProfile(CurrentUserId));

            return await GetUserProfileAsync(CurrentUserId);
        }

        public async Task<string> UploadAvatarAsync(int currentUserId, Stream fileStream, string fileName)
        {
            var user =await _unitOfWork.Users.GetByIdAsync(currentUserId)
                ?? throw new Exception("User not found");

            if(!string.IsNullOrEmpty(user.ProfilePictureUrl))
                _fileService.deleteFile(user.ProfilePictureUrl);

            var url = await _fileService.SaveFileAsync(fileStream, fileName,"avatars");

            user.ProfilePictureUrl = url;

             _unitOfWork.Users.Update(user);

            await _cacheService.RemoveAsync(CacheKeys.UserProfile(currentUserId));

            return url;
            
        }
    }
}
