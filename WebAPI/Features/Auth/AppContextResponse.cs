namespace WebAPI.Features.Auth
{
    public class AppContextResponse
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public UserReponse User { get; set; } = null!;
    }
    public class UserReponse
    {
        public string Email { get; set; } = null!;
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public List<string> Roles { get; set; } = new List<string>();
    }

}
