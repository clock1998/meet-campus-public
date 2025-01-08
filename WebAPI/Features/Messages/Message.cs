using WebAPI.Features.Users;
using WebAPI.Infrastructure;

namespace WebAPI.Features.Messages
{
    public class Message : Entity
    {
        public string Content { get; set; } = null!;
        public Guid ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
