using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Core.CloudflareCallAppClient;

namespace Core
{
    public class CloudflareCallAppClient
    {
        private readonly string _prefixPath;
        private readonly HttpClient _httpClient;
        private string _sessionId = string.Empty;
        // This is the App Id provided by the dashboard that identifies this Calls Application.
        private string appId = "asd";
        // DO NOT USE YOUR SECRET IN THE BROWSER FOR PRODUCTION. It should be kept and used server-side.
        private string appSecret = "asd";
        public CloudflareCallAppClient(string basePath = "https://rtc.live.cloudflare.com/v1")
        {
            _prefixPath = $"{basePath}/apps/{appId}";
            _httpClient = new HttpClient();
        }

        private async Task<T?> SendRequestAsync<T>(string url, object body, HttpMethod method)
        {
            var request = new HttpRequestMessage(method, url){};
            if (body != null)
            {
                var serializedBody = JsonSerializer.Serialize(body, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                });
                request.Content = new StringContent(serializedBody, Encoding.UTF8, "application/json");
            }
            
            request.Headers.Add("Authorization", $"Bearer {appSecret}");

            var response = await _httpClient.SendAsync(request);
            //response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T?>(responseString, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
        }

        private void CheckErrors(dynamic result, int tracksCount = 0)
        {
            try
            {
                if (result.errorCode != null)
                {
                    throw new Exception(result.errorDescription.ToString());
                }

                for (int i = 0; i < tracksCount; i++)
                {
                    if (result.tracks[i].errorCode != null)
                    {
                        throw new Exception($"tracks[{i}]: {result.tracks[i].errorDescription.ToString()}");
                    }
                }
            }
            catch (RuntimeBinderException)
            {
            }

        }

        public async Task<Session> NewSessionAsync(string offerSDP)
        {
            var url = $"{_prefixPath}/sessions/new";
            var body = new
            {
                sessionDescription = new
                {
                    type = "offer",
                    sdp = offerSDP
                }
            };

            var result = await SendRequestAsync<Session>(url, body, HttpMethod.Post);
            CheckErrors(result);
            return result;
        }

        public async Task<TracksResponse> NewTracksAsync(string sessionId, object[] trackObjects, string? offerSDP = null)
        {
            var url = $"{_prefixPath}/sessions/{sessionId}/tracks/new";
            if (string.IsNullOrEmpty(offerSDP))
            {
                var body = new
                {
                    Tracks = trackObjects
                };

                var result = await SendRequestAsync<TracksResponse>(url, body, HttpMethod.Post);
                CheckErrors(result, trackObjects.Length);
                return result;
            }
            else
            {
                var body = new Body
                {
                    SessionDescription = new SessionDescription
                    {
                        Type = "offer",
                        Sdp = offerSDP
                    },
                    Tracks = trackObjects
                };

                var result = await SendRequestAsync<TracksResponse>(url, body, HttpMethod.Post);
                CheckErrors(result, trackObjects.Length);
                return result;
            }
        }

        public async Task SendAnswerSDPAsync(string answer, string sessionId)
        {
            var url = $"{_prefixPath}/sessions/{sessionId}/renegotiate";
            var body = new Body
            {
                SessionDescription = new SessionDescription
                {
                    Type = "answer",
                    Sdp = answer
                }
            };

            var result = await SendRequestAsync<dynamic>(url, body, HttpMethod.Put);
            CheckErrors(result);
        }
        public async Task<GetTracksBySessionIdResponse> GetTracksBySessionIdAsync(string sessionId)
        {
            var url = $"{_prefixPath}/sessions/{sessionId}";

            var result = await SendRequestAsync<GetTracksBySessionIdResponse>(url, null, HttpMethod.Get);
            CheckErrors(result);
            return result;
        }

        public async Task<CloseTracksResponse> CloseTracksAsync(string sessionId, List<string> trackIds, string sdp, bool force = false)
        {
            var url = $"{_prefixPath}/sessions/{sessionId}/tracks/close";
            var body = new
            {
                tracks = trackIds,
                sessionDescription = new
                {
                    Type = "offer",
                    Sdp = sdp
                },
                force
            };

            var result = await SendRequestAsync<CloseTracksResponse>(url, body, HttpMethod.Put);
            CheckErrors(result);
            return result;
        }
        public class CloseTracksResponse
        {
            public string SessionDescription { get; set; } = null!;
            public bool RequiresImmediateRenegotiation { get; set; }
            public List<string> tracks { get; set; } = new List<string>();
        }
        public class GetTracksBySessionIdResponse
        {
            public string SessionId { get; set; } = null!;
            public List<GetTracksBySessionIdResponseTrack> tracks { get; set; } = new List<GetTracksBySessionIdResponseTrack>();
        }
        public class GetTracksBySessionIdResponseTrack
        {
            public string Location { get; set; } = null!;
            public string Mid { get; set; } = null!;
            public string? SessionId { get; set; } 
            public string TrackName { get; set; } = null!;
            public string Status { get; set; } = null!;
        }
        public class TracksResponse
        {
            public bool RequiresImmediateRenegotiation { get; set; }
            public List<Track> Tracks { get; set; } = new List<Track>();
            public SessionDescription SessionDescription { get; set; } = null!;
        }
        public class Track
        {
            public string? SessionId { get; set; }
            public string TrackName { get; set; } = null!;
            public string Mid { get; set; } = null!;
        }
        public class Session
        {
            public SessionDescription SessionDescription { get; set; } = null!;
            public string SessionId { get; set; } = null!;
        }
        public class Body
        {
            public SessionDescription? SessionDescription { get; set; }
            public object[]? Tracks { get; set; }
        }
        public class SessionDescription
        {
            public string Type { get; set; } = null!;
            public string Sdp { get; set; } = null!;
        }
    }

}
