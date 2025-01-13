using WebAPI.Features.UserProfiles;

namespace WebAPI.Features.Images
{
    public class ProfileImage : Image
    {
        public Guid UserProfileId { get; set; }
        public virtual UserProfile UserProfile { get; set; } = null!;
    }
}
