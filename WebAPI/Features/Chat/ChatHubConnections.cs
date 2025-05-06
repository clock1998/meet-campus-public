namespace WebAPI.Features.Chat
{
    public class ChatHubConnections
    {
        //userid = connectionid
        private static Dictionary<string, List<string>> _users = new();

        public static bool HasUserConnection(string userId, string connectionId)
        {
            try
            {
                if (_users.TryGetValue(userId, out var user))
                {
                    return user.Any(p => p.Contains(connectionId));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public static bool HasUser(string userId)
        {
            try
            {
                if (_users.TryGetValue(userId, out var user))
                {
                    return user.Any();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public static void AddUserConnection(string userId, string connectionId)
        {
            if (!string.IsNullOrEmpty(userId) && !HasUserConnection(userId, connectionId))
            {
                if (_users.ContainsKey(userId))
                    _users[userId].Add(connectionId);
                else
                    _users.Add(userId, [connectionId]);
            }
        }

        public static List<string> GetOnlineUsers()
        {
            return _users.Keys.ToList();
        }


        public static List<string> GetOnlineUserSessions(string userId)
        {
            return _users[userId];
        }
    }
}
