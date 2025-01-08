using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Image
{
    public class ImageUploadDTO
    {
        [Required]
        public ImageDTO[] Images { get; set; }
    }
}
