using static Core.CloudflareCallAppClient;

namespace Template.WebAPI.Hubs
{
    public class CloudflareConnections
    {
        //userid = connectionid
        public static Dictionary<(string connectionId, string sessionId), List<Track>> Connections { get; set; } = new();
        public static Dictionary<string, string> Users { get; set; } = new();

        public static void AddConnection(string connectionId, string sessionId, List<Track> tracks)
        {

            if (!string.IsNullOrEmpty(sessionId) && !string.IsNullOrEmpty(connectionId) && tracks.Any())
            {
                if (!Connections.ContainsKey((connectionId, sessionId)))
                    Connections.Add((connectionId, sessionId), tracks);
            }
        }

        public static List<(string, string)> ConnectionKeys()
        {
            return Connections.Keys.ToList();
        }
    }
}
