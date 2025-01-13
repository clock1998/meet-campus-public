using Microsoft.AspNetCore.Mvc;

namespace Template.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        public TestController()
        {
            
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Text = "Success!" });
        }
        //private readonly IImageRepository imageRepository;

        //public ImageController(IImageRepository imageRepository)
        //{
        //    this.imageRepository = imageRepository;
        //}

        //[HttpPost, Route("Upload")]
        //public async Task<IActionResult> Upload([FromForm] ImageUploadDTO imageUploadDTO)
        //{
        //    var modelState = await FileHelpers.CheckFileAsync(imageUploadDTO.File, ModelState, new string[] { ".jpg", ".png", ".jpeg" }, 52428800);
        //    if (!modelState.IsValid) 
        //    { 
        //        return BadRequest(modelState);
        //    }
        //    var image = new ProfileImage
        //    {
        //        File = imageUploadDTO.File,
        //        FileName = imageUploadDTO.FileName,
        //        FileDescription = imageUploadDTO.FileDescription,
        //        FileExtension = Path.GetExtension(imageUploadDTO.File.FileName),
        //        FileSizeInBytes = imageUploadDTO.File.Length,
        //    };
        //    image = await imageRepository.Upload(image);
        //    return Ok(image);
        //}
    }
}
