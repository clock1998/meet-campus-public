using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using WebAPI.Infrastructure;

namespace WebAPI.Features.Images
{
    public abstract class Image : Entity
    {
        [NotMapped]
        public IFormFile File { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string? FileDescription { get; set; }
        public string FileExtension { get; set; } = null!;
        public long FileSizeInBytes { get; set; }
        public string FilePath { get; set; } = null!;
    }
}
