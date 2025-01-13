using System.Net;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Images.Command
{
    public record ImageFile(IFormFile File, string? FileDescription);
    public abstract record ImageUploadRequest(ImageFile[] Images);
    public class UploadImageHandler<T>(AppDbContext context, HttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment) : Handler(context) where T : Image
    {
        public virtual async Task<T> HandleAsync(T image)
        {
            var trustedFileNameForDisplay = WebUtility.HtmlEncode(Path.GetFileNameWithoutExtension(image.File.FileName));
            if (!string.IsNullOrEmpty(image.FileName))
            {
                trustedFileNameForDisplay = WebUtility.HtmlEncode(image.FileName);
            }
            image.FileName = trustedFileNameForDisplay;
            image.FilePath = $"";
            // create the image in the data base first to get the id.
            await _context.AddAsync(image);
            await _context.SaveChangesAsync();

            var request = httpContextAccessor.HttpContext?.Request;
            image.FilePath = $"{request?.Scheme}://{request?.Host}{request?.PathBase}/Images/{image.Id}{image.FileExtension}";
            // create the file path using the generated id to avoid duplicate names.
            await _context.SaveChangesAsync();
            var localFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "Images", $"{image.Id}{image.FileExtension}");
            using (var fileStream = new FileStream(localFilePath, FileMode.Create))
            {
                await image.File.CopyToAsync(fileStream);
            }
            return image;
        }
    }

    //[Route("api/[controller]")]
    //[ApiController]
    //public class ImageController(CreateImageHandler _hander) : Controller
    //{
    //    [HttpPost, Route("Upload")]
    //    public async Task<IActionResult> Upload([FromForm] ImageUploadDTO imageUploadDTO)
    //    {
    //        var modelState = await FileHelpers.CheckFileAsync(imageUploadDTO.File, ModelState, new string[] { ".jpg", ".png", ".jpeg" }, 52428800);
    //        if (!modelState.IsValid)
    //        {
    //            return BadRequest(modelState);
    //        }
    //        var image = new ProfileImage
    //        {
    //            File = imageUploadDTO.File,
    //            FileName = imageUploadDTO.FileName,
    //            FileDescription = imageUploadDTO.FileDescription,
    //            FileExtension = Path.GetExtension(imageUploadDTO.File.FileName),
    //            FileSizeInBytes = imageUploadDTO.File.Length,
    //        };
    //        image = await _hander.HandleAsync(image);
    //        return Ok(image);
    //    }
    //}
}
