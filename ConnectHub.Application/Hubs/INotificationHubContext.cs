using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Hubs
{
    public interface INotificationHubContext
    {
        Task SendNotificationAsync(int userId, object notification);
    }
}
