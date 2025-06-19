using System.ComponentModel.DataAnnotations;
using WebAPI.Features.Auth;
using WebAPI.Infrastructure;
using WebAPI.Features.Chat.ChatMessage;

namespace WebAPI.Features.Chat.ChatRoom
{
    public class Room : Entity
    {
        [MaxLength(50)]
        public string Name { get; set; } = null!;
        
        public virtual List<ApplicationUser> ApplicationUsers { get; set; } = new List<ApplicationUser>();
        
        public virtual List<Message> Messages { get; set; } = new();
    }
}
