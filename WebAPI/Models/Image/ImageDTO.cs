using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Image
{
    public class ImageDTO
    {
        [Required]
        public IFormFile File { get; set; }
        public string? FileDescription { get; set; }

    }
}
