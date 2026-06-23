using ConnectHub.Application.DTO_s.Notifications;
using ConnectHub.Application.DTO_s.Posts;
using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, NotificationType type, string message);
        Task<PaginatedResponseDto<NotificationDto>> GetNotificationsAsync(int userId, int pageNumber, int pageSize);
        Task MarkAsReadAsync(int notificationId,int userId);
        Task MarkAllAsReadAsync(int userId);

        Task PushNotificationAsync(int userId, NotificationDto notification);
    }
}
