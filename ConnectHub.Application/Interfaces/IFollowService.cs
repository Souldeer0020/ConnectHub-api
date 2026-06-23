using ConnectHub.Application.DTO_s.Follows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Interfaces
{
    public interface IFollowService
    {
        Task<string> FollowAsync(int followerId, int followingId);
        Task<string> UnfollowAsync(int followerId, int followingId);
        Task<IReadOnlyList<FollowUserDto>> GetFollowersAsync(int UserId);
        Task<IReadOnlyList<FollowUserDto>> GetFollowingAsync(int UserId);
        Task<FollowStatsDto> GetFollowStatsAsync(int CurrentUserId, int TargetUserId);
    }
}
