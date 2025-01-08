using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Core;
using static Core.CloudflareCallAppClient;
using WebAPI.Infrastructure.Context;
namespace Template.WebAPI.Hubs
{
    public interface ICloudflareClient
    {
        Task NewSessionCreated(string sessionId, List<Track> tracks);
        Task UsersConnected(List<string> emails);
        Task AddedToRoom(string sessionId, List<Track> tracks);
        Task NotEnoughConnections();
    }
    [Authorize]
    public class CloudflareHub : Hub<ICloudflareClient>
    {
        private readonly AppDbContext _context;
        public CloudflareCallAppClient cloudflareCallAppClient = new CloudflareCallAppClient();
        public CloudflareHub(AppDbContext context )
        {
            _context = context;
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var email = _context.Users.FirstOrDefault(n => n.Id.ToString() == userId)?.Email;
                if (!string.IsNullOrEmpty(email))
                {
                    var addSuccess = CloudflareConnections.Users.TryAdd(Context.ConnectionId, email);
                    if (addSuccess)
                    {
                         Clients.All.UsersConnected(CloudflareConnections.Users.Values.ToList());
                    }
                }
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            //Remove User and Connections
            CloudflareConnections.Users.Remove(Context.ConnectionId);
            var keysToRemove = CloudflareConnections.Connections.Keys.Where(n => n.connectionId == Context.ConnectionId);
            
            foreach (var keyToRemove in keysToRemove) {
                CloudflareConnections.Connections.Remove(keyToRemove);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public void NewSession(string sessionId, List<Track> tracks)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = _context.Users.FirstOrDefault(n => n.Id.ToString() == userId)?.Email;
            CloudflareConnections.Connections.TryAdd((Context.ConnectionId, sessionId), tracks);
        }
    }
}
