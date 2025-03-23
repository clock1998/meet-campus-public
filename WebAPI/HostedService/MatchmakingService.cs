using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using WebAPI.Features.Cloudflare;

namespace Template.WebAPI.HostedService
{
    public class MatchmakingService : BackgroundService
    {
        private Timer? _timer = null;
        private int executionCount = 0;
        private readonly IHubContext<CloudflareHub, ICloudflareClient> _hubContext;
        private readonly ILogger<MatchmakingService> _logger;
        private readonly Random random = new Random();
        public MatchmakingService(IHubContext<CloudflareHub, ICloudflareClient> hubContext, ILogger<MatchmakingService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // When the timer should have no due-time, then do the work once now.
            await DoWork();

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(0.1));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await DoWork();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Timed Hosted Service is stopping.");
            }
        }

        private async Task DoWork()
        {
            try
            {
                //var count = Interlocked.Increment(ref executionCount);
                //var room = Guid.NewGuid().ToString(); // Correct way to create a new Guid
                if (CloudflareConnections.Connections.Count >= 2)
                {
                    // Select two random connections
                    var connectionKeys = CloudflareConnections.Connections.Keys.ToList();
                    var firstIndex = random.Next(connectionKeys.Count);
                    var firstConnectionKey = connectionKeys[firstIndex];

                    connectionKeys.RemoveAt(firstIndex);
                    var secondIndex = random.Next(connectionKeys.Count);
                    var secondConnectionKey = connectionKeys[secondIndex];

                    // Add the two connections to the room
                    //await _hubContext.Groups.AddToGroupAsync(firstConnectionKey.connectionId, room);
                    //await _hubContext.Groups.AddToGroupAsync(secondConnectionKey.connectionId, room);
                    await _hubContext.Clients.Client(firstConnectionKey.connectionId).AddedToRoom(secondConnectionKey.sessionId, CloudflareConnections.Connections[secondConnectionKey]);
                    await _hubContext.Clients.Client(secondConnectionKey.connectionId).AddedToRoom(firstConnectionKey.sessionId, CloudflareConnections.Connections[firstConnectionKey]);
                    CloudflareConnections.Connections.Remove(firstConnectionKey);
                    CloudflareConnections.Connections.Remove(secondConnectionKey);
                    // Notify both connections
                    //await _hubContext.Clients.Group(room).AddedToRoom(
                    //    firstConnectionKey.sessionId, CloudflareConnections.Connections[firstConnectionKey],
                    //    secondConnectionKey.sessionId, CloudflareConnections.Connections[firstConnectionKey]);
                    //await Clients.Client(Connections[firstConnection].First().ConnectionId).SendAsync("AddedToRoom", room);
                    //await Clients.Client(Connections[secondConnection].First().ConnectionId).SendAsync("AddedToRoom", room);
                }
                else
                {
                    // Notify that there are not enough connections to start a match
                    //_logger.LogInformation("there are not enough connections to start a match");
                    //await Clients.Caller.NotEnoughConnections();
                }
                //_logger.LogInformation("Timed Hosted Service is working. Count: {Count}", count);
            }
            catch (Exception e)
            {
                _logger.LogInformation($"MatchmakingServiceException: {e.Message}" );
            }
            
        }

        //public Task StopAsync(CancellationToken cancellationToken)
        //{
        //    _logger.LogInformation("Timed Hosted Service is stopping.");

        //    _timer?.Change(Timeout.Infinite, 0);

        //    return Task.CompletedTask;
        //}

        //public void Dispose()
        //{
        //    _timer?.Dispose();
        //}
    }
}
