using WebAPI.Features.Auth;

namespace WebAPI.Features.Chat
{
    public class ChatHubConnections
    {
        private static Dictionary<ApplicationUser, List<string>> _users = new();
        
        public static bool HasUserConnection(ApplicationUser user, string connectionId)
        {
            try
            {
                if (_users.TryGetValue(user, out var appUser))
                {
                    return appUser.Any(p => p.Contains(connectionId));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public static bool HasUser(ApplicationUser user)
        {
            try
            {
                if (_users.TryGetValue(user, out var appUser))
                {
                    return appUser.Any();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public static void AddUserConnection(ApplicationUser user, string connectionId)
        {
            if (!HasUserConnection(user, connectionId))
            {
                if (_users.ContainsKey(user))
                    _users[user].Add(connectionId);
                else
                    _users.Add(user, [connectionId]);
            }
        }

        public static List<ApplicationUser> GetOnlineUsers()
        {
            return _users.Keys.ToList();
        }

        public static ApplicationUser GetOnlineUser(string connectionId)
        {
            return _users.FirstOrDefault(n => n.Value.Any(p => p.Contains(connectionId))).Key;
        }
        public static List<string> GetOnlineUserSessions(ApplicationUser user)
        {
            return _users[user];
        }
        
        public static List<string> GetOnlineUserSessions(Guid userId)
        {
            return _users[new ApplicationUser(){Id = userId}];
        }
    }
}
