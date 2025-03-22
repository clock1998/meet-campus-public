using WebAPI.Features.Auth;
using WebAPI.Infrastructure;

namespace WebAPI.Features.Chat.ChatRoom
{
    public class RoomUser : Entity
    {
        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;
        public Guid RoomId { get; set; }
        public virtual Room Room { get; set; } = null!;
    }
}
