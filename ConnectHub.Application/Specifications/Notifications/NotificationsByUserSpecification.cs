using ConnectHub.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Specifications.Notifications
{
    public class NotificationsByUserSpecification :BaseSpecification<Notification>
    {
        public NotificationsByUserSpecification(int userId,int pageNumber,int pageSize)
        {
            AddCriteria(n=>n.UserId == userId  && n.IsRead==false);
            AddOrderByDescending(n=>n.CreatedAt);
            ApplyPagination(pageNumber, pageSize);
        }
    }
}
