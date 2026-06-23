using ConnectHub.Application.DTO_s.Notifications;
using ConnectHub.Application.DTO_s.Posts;
using ConnectHub.Application.Hubs;
using ConnectHub.Application.Interfaces;
using ConnectHub.Application.Specifications.Notifications;
using ConnectHub.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationHubContext _hubContext;

        public NotificationService(IUnitOfWork unitOfWork,INotificationHubContext hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        public async Task CreateNotificationAsync(int userId, NotificationType type, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Message = message,
            };

            
            //await PushNotificationAsync(userId, MapToDto(notification));

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            await _hubContext.SendNotificationAsync(userId, MapToDto(notification));
        }

        public async Task<PaginatedResponseDto<NotificationDto>> GetNotificationsAsync(int userId, int pageNumber, int pageSize)
        {
            Console.WriteLine($"Getting notifications for userId: {userId}");
            var spec = new NotificationsByUserSpecification(userId, pageNumber, pageSize);
            var countSpec = new NotificationsCountSpecification(userId);

            var notifications = await _unitOfWork.Notifications.ListBySpecAsync(spec);
            var totalCount = await _unitOfWork.Notifications.CountBySpecAsync(countSpec);

            return new PaginatedResponseDto<NotificationDto>
            {
                Data=notifications.Select(MapToDto).ToList(),
                PageNumber=pageNumber,
                PageSize = pageSize,
                TotalCount=totalCount
            };
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var spec = new UnreadNotificationsSpecification(userId);
            var unReadNotifications = await _unitOfWork.Notifications.ListBySpecAsync(spec);

            foreach (var notificaion in unReadNotifications)
            {
                notificaion.IsRead = true;
                _unitOfWork.Notifications.Update(notificaion);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId) ??
                throw new Exception("No notification were found");

            if (notification.UserId != userId)
                throw new UnauthorizedAccessException("You can mark ony your notifications as read");

            notification.IsRead = true;

            _unitOfWork.Notifications.Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        private static NotificationDto MapToDto(Notification n)
        {
            return new NotificationDto
            {
                Id = n.Id,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead,
                Message = n.Message,
                Type = n.Type.ToString()
            };
        }

        public async Task PushNotificationAsync(int userId, NotificationDto notification)
        {
            await _hubContext.SendNotificationAsync(userId, notification);
        }
    }
}
