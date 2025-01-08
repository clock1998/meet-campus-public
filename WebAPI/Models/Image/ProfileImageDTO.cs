using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Image
{
    public class ProfileImageDTO : ImageUploadDTO
    {
        [Required]
        public string UserProfileId { set; get; }
    }
}
