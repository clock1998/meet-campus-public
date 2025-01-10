using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Template.WebAPI.Hubs;
using Core;
using WebAPI.Infrastructure.Context;
using WebAPI.Features.Auth;

namespace Template.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CloudflareController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly UserManager<ApplicationUser> _userManager;
        private CloudflareCallAppClient cloudflareCallAppClient;
        private static List<string> activeSessions = new List<string>();
        private readonly IHubContext<WebRTCHub> _hubContext;
        public CloudflareController(AppDbContext context, UserManager<ApplicationUser> userManager, IHubContext<WebRTCHub> hubContext)
        {
            this.context = context;
            this._userManager = userManager;
            this.cloudflareCallAppClient = new CloudflareCallAppClient();
            this._hubContext = hubContext;
        }

        public class NewSessionRequest
        {
            public string Sdp { get; set; } = null!;
        }

        [HttpPost, Route("NewSession")]
        public async Task<IActionResult> NewSession([FromBody] NewSessionRequest model)
        {
            var session = await cloudflareCallAppClient.NewSessionAsync(model.Sdp);
            await _userManager.GetUserAsync(User);
            activeSessions.Add(session.SessionId);
            return Ok(session);
        }
        public class TracksRequest
        {
            public string SessionId { get; set; } = null!;
            public string? Sdp { get; set; }
            public object[] Tracks { get; set; } = null!;
        }
        [HttpPost, Route("NewTracks")]
        public async Task<IActionResult> NewTracks([FromBody] TracksRequest model)
        {
            var trackResult = await cloudflareCallAppClient.NewTracksAsync(model.SessionId, model.Tracks, model.Sdp);
            return Ok(trackResult);
        }
        public class CloseTracksRequest
        {
            public string SessionId { get; set; } = null!;
            public List<string> Tracks { get; set; } = null!;
            public string Sdp{ get; set; } = null!;
            public bool Force {  get; set; }
        }

        [HttpPut, Route("CloseTracks")]
        public async Task<IActionResult> CloseTracks([FromBody] CloseTracksRequest model)
        {
            return Ok(await cloudflareCallAppClient.CloseTracksAsync(model.SessionId, model.Tracks, model.Sdp!, model.Force));
        }
        public class SendAnswerSDPRequest
        {
            public string SessionId { get; set; } = null!;
            public string Sdp { get; set; } = null!;
        }
        [HttpPut, Route("SendAnswerSDP")]
        public async Task<IActionResult> SendAnswerSDP([FromBody] SendAnswerSDPRequest model)
        {
            await cloudflareCallAppClient.SendAnswerSDPAsync(model.Sdp, model.SessionId!);
            return Ok();
        }

        //[HttpPut, Route("CloseTracks")]
        //public async Task<IActionResult> CloseTracks([FromBody] TrackViewModel model)
        //{
        //    var result = await cloudflareCallAppClient.CloseTracksAsync(model.SessionId!, model.Tracks, model.Sdp!, model.Force);
        //    activeSessions.Remove(model.SessionId);
        //    return Ok(result);
        //}

        //[HttpGet, Route("GetSessionState/{sessionId}")]
        //public async Task<IActionResult> GetSessionState([FromRoute] string sessionId)
        //{
        //    var result = await cloudflareCallAppClient.GetSessionStateAsync(sessionId);
        //    return Ok(result);
        //}

    }
}
