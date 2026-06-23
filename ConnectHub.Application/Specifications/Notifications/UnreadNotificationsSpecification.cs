using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Specifications.Notifications
{
    public class UnreadNotificationsSpecification :BaseSpecification<Notification>
    {
        public UnreadNotificationsSpecification(int userId)
        {
            AddCriteria(n => n.UserId == userId && !n.IsRead);
            AddOrderByDescending(n => n.CreatedAt);
        }
    }
}
