using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.Json;
using static Template.WebAPI.Hubs.WebRTCHub;
using WebAPI.Infrastructure.Context;
namespace Template.WebAPI.Hubs
{
    public interface IChatClient
    {
        Task QueueJoined(List<string> user);
        Task ReceivedIceCandidateFromServer(IceCandidate iceCandidate);
        Task NewOfferAwaiting(Transaction transaction);
        Task AnswererResponse(Transaction transaction);
    }

    [Authorize]
    public class WebRTCHub : Hub<IChatClient>
    {
        private readonly AppDbContext _context;
        //private readonly IMessageRepository messageRepository;

        public static int ActiveUsers { get; set; } = 0;
        private string groupName = "Queue";
        public static List<Transaction> Transactions { get; set; } = new();
        public WebRTCHub(AppDbContext context )
        {
            _context = context;
        }
        
        public class Transaction
        {
            public string? OfferUsername { get; set; }
            public string Offer { get; set; } = null!;
            public List<string> OfferIceCandidates { get; set; } = new();
            public string? AnswerUsername { get; set; }
            public string? Answer { get; set; }
            public List<string> AnswerIceCandidates { get; set; } = new();
        }
        public class IceCandidate
        {
            public string Email { get; set; } = null!;
            public string Candiate { get; set; } = null!;
            public bool IsMyOffer { get; set; }
        }

        //Get connected user number
        //public override Task OnConnectedAsync()
        //{
        //    var userId = Context.User?.FindFirstValue(ClaimTypes.Name);
        //    if (!string.IsNullOrEmpty(userId))
        //    {
        //        var email = _context.Users.FirstOrDefault(n => n.Id.ToString() == userId)?.Email;
        //        if(!string.IsNullOrEmpty(email))
        //            HubConnections.AddUserConnection(email, Context.ConnectionId);
        //    }
        //    Clients.Users(HubConnections.OnlineUsers()).SendAsync("UserConnected", HubConnections.OnlineUsers().Count);
        //    return base.OnConnectedAsync();
        //}

        //public override Task OnDisconnectedAsync(Exception? exception)
        //{
        //    var userId = Context.User?.FindFirstValue(ClaimTypes.Name);
        //    var email = _context.Users.FirstOrDefault(n => n.Id.ToString() == userId)?.Email;
        //    if (!string.IsNullOrEmpty(email) && HubConnections.HasUserConnection(email, Context.ConnectionId))
        //    {
        //        var userConnections = HubConnections.Users[email];
        //        userConnections.Remove(Context.ConnectionId);
        //        HubConnections.Users.Remove(email);
        //    }
        //    return base.OnDisconnectedAsync(exception);
        //}

        ////User click button to join queue
        //public async Task JoinQueue()
        //{
        //    var userId = Context.User?.FindFirstValue(ClaimTypes.Name);
        //    var email = _context.Users.FirstOrDefault(n => n.Id.ToString() == userId)?.Email;
        //    if (!string.IsNullOrEmpty(email))
        //    {
        //        HubConnections.AddUserConnection(email, Context.ConnectionId);
        //        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        //        await Clients.Group(groupName).QueueJoined(HubConnections.OnlineUsers());
        //    }
        //}
        //public async Task AddIceCandidate(IceCandidate iceCandidate)
        //{
        //    if (iceCandidate.IsMyOffer)
        //    {
        //        //this ice is coming from the offerer. Send to the answerer
        //        var transaction = Transactions.FirstOrDefault(n => n.OfferUsername == iceCandidate.Email);
        //        if (transaction != null)
        //        {
        //            // 1. When the answerer answers, all existing ice candidates are sent
        //            // 2. Any candidates that come in after the offer has been answered, will be passed through
        //            transaction.OfferIceCandidates.Add(iceCandidate.Candiate);
        //            if (!string.IsNullOrEmpty(transaction.AnswerUsername))
        //            {
        //                //pass it through to the other socket
        //                var connectionId = HubConnections.Users[transaction.AnswerUsername].FirstOrDefault();
        //                if (!string.IsNullOrEmpty(connectionId))
        //                {
        //                    await Clients.Client(connectionId).ReceivedIceCandidateFromServer(iceCandidate);
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        //this ice is coming from the answerer. Send to the offerer
        //        //pass it through to the other socket
        //        var transaction = Transactions.FirstOrDefault(n => n.AnswerUsername == iceCandidate.Email);
        //        if (transaction != null)
        //        {
        //            var connectionIds = HubConnections.Users[transaction.OfferUsername!];
        //            if (connectionIds != null && connectionIds.Any())
        //            {
        //                await Clients.Clients(connectionIds).ReceivedIceCandidateFromServer(iceCandidate);
        //            }
                        
        //        }
        //    }
        //}
        //public async Task SendOffer(string offer)
        //{
        //    var userId = Context.User?.FindFirstValue(ClaimTypes.Name);
        //    var email = _context.Users.FirstOrDefault(n => n.Id.ToString() == userId)?.Email;
        //    var newTransaction = new Transaction {OfferUsername = email, Offer = offer };
        //    Transactions.Add(newTransaction);
        //    //send to the group except myself.
        //    await Clients.GroupExcept(groupName, HubConnections.Users[email]).NewOfferAwaiting(newTransaction);
        //}

        //public async Task SendAnswer(string transactionInString)
        //{
        //    Transaction transaction = JsonSerializer.Deserialize<Transaction>(transactionInString, new JsonSerializerOptions
        //    {
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //    })!;
        //    var email = HubConnections.OnlineUsers().FirstOrDefault(n => n == transaction.OfferUsername);
        //    var conntectIds = HubConnections.Users[email];
        //    if (email == null && !conntectIds.Any())
        //    {
        //        return;
        //    }
        //    var transactionToUpdate = Transactions.FirstOrDefault(n => n.OfferUsername == transaction.OfferUsername);
        //    if (transactionToUpdate == null)
        //    {
        //        return;
        //    }
        //    transactionToUpdate.Answer = transaction.Answer;
        //    transactionToUpdate.AnswerUsername = transaction.AnswerUsername;
        //    //send back the method caller icecandidates
        //    await Clients.Caller.AnswererResponse(transactionToUpdate);
        //    //send offerer transaction.
        //    await Clients.Clients(conntectIds).AnswererResponse(transactionToUpdate);
        //}

        //when a room is selected, we call add to room
        //public async Task AddToRoom(string roomId)
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

        //    await Clients.Group(roomId).SendAsync("AddToRoom", $"{Context.ConnectionId} has joined the group {roomId}.");
        //}
        //when a room is unselected, we call remove from room
        //public async Task RemoveFromRoom(string roomId)
        //{
        //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

        //    //await Clients.Group(roomId).SendAsync("Send", $"{Context.ConnectionId} has left the group {roomId}.");
        //}

        //public override Task OnConnectedAsync()
        //{
        //    var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (!string.IsNullOrEmpty(userId))
        //    {
        //        var userName = context.Users.FirstOrDefault(n => n.Id == userId).UserName;
        //        Clients.Users(HubConnections.OnlineUsers()).SendAsync("ReceiveUserConnected", userId, userName);
        //        HubConnections.AddUserConnection(userId, Context.ConnectionId);
        //    }
        //    return base.OnConnectedAsync();
        //}

        //public override Task OnDisconnectedAsync(Exception? exception)
        //{
        //    var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (HubConnections.HasUserConnection(userId, Context.ConnectionId))
        //    {
        //        var userConnections = HubConnections.Users[userId];
        //        userConnections.Remove(Context.ConnectionId);
        //        //HubConnections.Users.Remove(userId);
        //    }
        //    if (!string.IsNullOrEmpty(userId))
        //    {
        //        var userName = context.Users.FirstOrDefault(n => n.Id == userId).UserName;
        //        Clients.Users(HubConnections.OnlineUsers()).SendAsync("ReceiveUserDisconnected", userId, userName, HubConnections.HasUser(userId));
        //        HubConnections.AddUserConnection(userId, Context.ConnectionId);
        //    }
        //    return base.OnDisconnectedAsync(exception);
        //}
        //public async Task SendMessageToReceiver(string sender, string receiver, string message)
        //{
        //    var userId =
        //}
    }
}
