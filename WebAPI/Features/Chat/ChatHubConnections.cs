using WebAPI.Features.Auth;

namespace WebAPI.Features.Chat
{
    public class User
    {
        public Guid Id { set; get; }
        public string Email { set; get; } = string.Empty;
        public string Username { set; get; } = string.Empty;

        public User(Guid id)
        {
            Id = id;
        }
        public User(ApplicationUser appUser)
        {
            Id = appUser.Id;
            Email = appUser.Email!;
            Username = appUser.UserName!;
        }
        public override bool Equals(object? obj) => Equals(obj as User);
        public override int GetHashCode() => Id.GetHashCode();

        public bool Equals(User? other) => other is not null && other.Id == Id;
    }
    public class ChatHubConnections
    {
        private static Dictionary<string, List<string>> _users = new();
        
        public static bool HasUserConnection(string userId, string connectionId)
        {
            try
            {
                if (_users.TryGetValue(userId, out var appUser))
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

        public static void AddUserConnection(string userId, string connectionId)
        {
            if (!HasUserConnection(userId, connectionId))
            {
                if (_users.ContainsKey(userId))
                    _users[userId].Add(connectionId);
                else
                    _users.Add(userId, [connectionId]);
            }
        }
        
        public static void RemoveUserConnection(string userId, string connectionId)
        {
            if (HasUserConnection(userId, connectionId))
            {
                _users[userId].Remove(connectionId);
            }
        }
        
        public static void RemoveUser(string userId)
        {
            _users.Remove(userId);
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
