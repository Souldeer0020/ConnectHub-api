using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Application.Constants
{
    public class CacheKeys
    {
        public static string UserProfile(int userId)
            => $"User:Profile:{userId}";
        public static string Post(int postId)
            => $"Post:{postId}";
        public static string Feed(int userId, int pageNumber, int pageSize)
            => $"Feed:{userId}:Page:{pageNumber}:Size:{pageSize}";
        public static string FeedPrefix(int userId)
            => $"Feed:{userId}";
    }
}
