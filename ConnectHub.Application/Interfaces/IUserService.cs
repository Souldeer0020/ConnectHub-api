using ConnectHub.Application.DTO_s.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task<UserProfileDto> UpdateProfileAsync(int CurrentUserId, UpdateProfileDto dto);
        Task<string> UploadAvatarAsync(int currentUserId, Stream fileStream, string fileName);
    }
}
