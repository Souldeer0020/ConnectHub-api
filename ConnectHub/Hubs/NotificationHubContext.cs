using ConnectHub.Application.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ConnectHub.Hubs
{
    public class NotificationHubContext : INotificationHubContext
    {
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationHubContext(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }
        public async Task SendNotificationAsync(int userId, object notification)
        {

            await _hub.Clients
                .Group($"user_{userId}")
                .SendAsync("ReceiveNotification", notification);

        }
    }
}
