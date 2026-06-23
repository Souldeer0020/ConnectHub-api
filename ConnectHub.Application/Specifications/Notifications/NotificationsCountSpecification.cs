using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Specifications.Notifications
{
    public class NotificationsCountSpecification :BaseSpecification<Notification>
    {
        public NotificationsCountSpecification(int userId)
        {
            AddCriteria(n => n.UserId == userId && n.IsRead==false);
            // No paging, no ordering — just the WHERE clause for COUNT(*)
        }
    }
}
