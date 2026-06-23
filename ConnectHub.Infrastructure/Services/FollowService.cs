using ConnectHub.Application.DTO_s.Follows;
using ConnectHub.Application.Interfaces;
using ConnectHub.Application.Specifications.Follows;
using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Infrastructure.Services
{
    public class FollowService : IFollowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public FollowService(IUnitOfWork unitOfWork,INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }
        public async Task<string> FollowAsync(int followerId, int followingId)
        {
            if(followerId == followingId)
                throw new InvalidOperationException("You cannot follow yourself.");

            var targetUser= await _unitOfWork.Users.GetByIdAsync(followingId)??
                throw new Exception("User does not even exist");

            var existSpec = new FollowExistsSpecification(followerId, followingId);
            var existingFollow = await _unitOfWork.Follows.GetBySpecAsync(existSpec);

            if(existingFollow != null)
                throw new Exception("You are already following this user.");

           
            var follow = new Follow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTime.UtcNow
            };

            var follower = await _unitOfWork.Users.GetByIdAsync(followerId);
            await _notificationService.CreateNotificationAsync(followingId, NotificationType.Follow, $"{follower!.UserName} started following you");

            await _unitOfWork.Follows.AddAsync(follow);
            await _unitOfWork.SaveChangesAsync();
            return targetUser.UserName;
        }

        public async Task<IReadOnlyList<FollowUserDto>> GetFollowersAsync(int UserId)
        {
            var spec = new FollowersSpecification(UserId);
            var followers = await _unitOfWork.Follows.ListBySpecAsync(spec);

            return followers.Select(f => new FollowUserDto // used for mapping from Follow entity to FollowUserDto
            {
                UserId = f.FollowerId,
                UserName = f.Follower.UserName,
                ProfilePictureUrl = f.Follower.ProfilePictureUrl,
                Bio = f.Follower.Bio
            }).ToList();
        }

        public async Task<IReadOnlyList<FollowUserDto>> GetFollowingAsync(int UserId)
        {
            var spec = new FollowingSpecification(UserId);
            var followings = await _unitOfWork.Follows.ListBySpecAsync(spec);

            return followings.Select(f => new FollowUserDto 
            {
                UserId = f.FollowingId,
                UserName = f.Following.UserName,
                ProfilePictureUrl = f.Following.ProfilePictureUrl,
                Bio = f.Following.Bio
            }).ToList();
        }

        public async Task<FollowStatsDto> GetFollowStatsAsync(int CurrentUserId, int TargetUserId)
            
            => new FollowStatsDto
            {
                IsFollowing = _unitOfWork.Follows.GetBySpecAsync(new FollowExistsSpecification(CurrentUserId, TargetUserId)).Result != null,
                FollowersCount = _unitOfWork.Follows.CountBySpecAsync(new FollowersSpecification(TargetUserId)).Result,
                FollowingCount = _unitOfWork.Follows.CountBySpecAsync(new FollowingSpecification(TargetUserId)).Result
            };
        

        public async Task<string> UnfollowAsync(int followerId, int followingId)
        {
            var existspec = new FollowExistsSpecification(followerId, followingId);
            var existingFollow =await _unitOfWork.Follows.GetBySpecAsync(existspec)??
                throw new Exception("You are not following this user.");

            _unitOfWork.Follows.Delete(existingFollow);  // update the state of the entity to Deleted, it will be removed from the database when SaveChangesAsync is called
            await _unitOfWork.SaveChangesAsync(); // commit the transaction and persist the changes to the database

            return existingFollow.Following.UserName;
        }
    }
}
