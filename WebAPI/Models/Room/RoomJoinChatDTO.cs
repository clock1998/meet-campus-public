using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Room
{
    public class RoomJoinChatDTO
    {
        public string[] UserIds { get; set; }
    }
}
