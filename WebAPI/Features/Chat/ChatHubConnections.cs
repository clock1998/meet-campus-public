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
        private static Dictionary<User, List<string>> _users = new();
        
        public static bool HasUserConnection(User user, string connectionId)
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

        public static bool HasUser(User user)
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

        public static void AddUserConnection(User user, string connectionId)
        {
            if (!HasUserConnection(user, connectionId))
            {
                if (_users.ContainsKey(user))
                    _users[user].Add(connectionId);
                else
                    _users.Add(user, [connectionId]);
            }
        }

        public static List<User> GetOnlineUsers()
        {
            return _users.Keys.ToList();
        }

        public static User GetOnlineUser(string connectionId)
        {
            return _users.FirstOrDefault(n => n.Value.Any(p => p.Contains(connectionId))).Key;
        }
        public static List<string> GetOnlineUserSessions(User user)
        {
            return _users[user];
        }
        
        public static List<string> GetOnlineUserSessions(Guid userId)
        {
            return _users[new User(userId)];
        }
    }
}
