using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using WebAPI.Features.Chat.ChatMessage;
using WebAPI.Features.Chat.ChatRoom;
using WebAPI.Features.Courses;
using WebAPI.Features.UserProfiles;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.Auth
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public byte[]? Avatar { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        [NotMapped]
        public string? AvatarDataUrl => Avatar?.ToDataImageUrl();


        public Guid UserProfileId { get; set; }
        public virtual UserProfile UserProfile { get; set; } = null!;

        public virtual ICollection<ApplicationUserClaim> Claims { get; set; } = new List<ApplicationUserClaim>();
        public virtual ICollection<ApplicationUserLogin> Logins { get; set; } = new List<ApplicationUserLogin>();
        public virtual ICollection<ApplicationUserToken> Tokens { get; set; } = new List<ApplicationUserToken>();
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
        public virtual ICollection<Course> Courses { set; get; } = new List<Course>();
        public virtual ICollection<Message> Messages { set; get; } = new List<Message>();
        public virtual ICollection<Room> Rooms { set; get; } = new List<Room>();

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) { return false; }
            if (obj.GetType() != GetType()) { return false; }
            if (obj is not ApplicationUser entity) { return false; }
            return entity.Id == Id;
        }

    }

    public class ApplicationRole : IdentityRole<Guid>
    {
        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
        public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; } = new List<ApplicationRoleClaim>();
    }

    public class ApplicationUserRole : IdentityUserRole<Guid>
    {
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ApplicationRole Role { get; set; } = null!;
    }

    public class ApplicationUserClaim : IdentityUserClaim<Guid>
    {
        public virtual ApplicationUser User { get; set; } = null!;
    }

    public class ApplicationUserLogin : IdentityUserLogin<Guid>
    {
        public virtual ApplicationUser User { get; set; } = null!;
    }

    public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
    {
        public virtual ApplicationRole Role { get; set; } = null!;
    }

    public class ApplicationUserToken : IdentityUserToken<Guid>
    {
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
