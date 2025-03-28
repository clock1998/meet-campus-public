﻿using WebAPI.Features.Auth;
using WebAPI.Infrastructure;
using WebAPI.Features.Chat.ChatRoom;

namespace WebAPI.Features.Chat.ChatMessage
{
    public class Message : Entity
    {
        public string Content { get; set; } = null!;
        public Guid ApplicationUserId { get; set; }
        public virtual ApplicationUser? ApplicationUser { get; set; }
        public Guid RoomId { get; set; }
        public virtual Room Room { get; set; } = null!;
    }
}
