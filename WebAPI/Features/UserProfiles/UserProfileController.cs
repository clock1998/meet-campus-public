using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Features.Images;
using WebAPI.Features.Images.Command;
using WebAPI.Features.UserProfiles.Command;
using WebAPI.Infrastructure.Context;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.UserProfiles
{
    [Route("api/[controller]"), ApiController, Authorize]
    public class UserProfileController(
        UploadProfileImageHandler uploadProfileImageHander, 
        DeleteImageHandler<ProfileImage> deleteImageHandler,
        UpdateUserProfileHandler updateUserProfileHandler,
        AppDbContext context) 
        : ControllerBase
    {
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var model = await context.UserProfiles.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAll([FromQuery] CourseParameters queryParameters)
        //{
        //    var courses = await respository.GetAllByConditionAsync(queryParameters);
        //    Response.Headers.Add("X-Pagination", courses.GeneratePagedMeta());
        //    //courses.Include(typeof(Semester).ToString()).Where
        //    return Ok(courses);
        //}
        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UserProfileRequest request)
        {
            return Ok(await updateUserProfileHandler.HandleAsync(id, request));
        }
        [HttpPost, Route("Upload")]
        public async Task<IActionResult> Upload([FromForm] ProfileImageUploadRequest imageUploadDTO)
        {
            var modelState = await FileHelpers.CheckFileAsync(imageUploadDTO.Images.Select(n => n.File).ToArray(), ModelState, new string[] { ".jpg", ".png", ".jpeg" }, 52428800);
            if (!modelState.IsValid)
            {
                return BadRequest(modelState);
            }
            var images = new List<ProfileImage>();
            foreach (var imageDTO in imageUploadDTO.Images)
            {
                images.Add(new ProfileImage
                {
                    File = imageDTO.File,
                    FileName = imageDTO.File.FileName,
                    FileDescription = imageDTO.FileDescription,
                    FileExtension = Path.GetExtension(imageDTO.File.FileName),
                    FileSizeInBytes = imageDTO.File.Length,
                    UserProfileId = Guid.Parse(imageUploadDTO.UserProfileId)
                });
            }
            foreach (var image in images)
            {
                await uploadProfileImageHander.HandleAsync(image);
            }

            return Ok(images);
        }

        [HttpDelete, Route("DeleteProfileImage/{id:Guid}")]
        public async Task<IActionResult> DeleteProfileImage([FromRoute] Guid id)
        {
            var model = await deleteImageHandler.HandlerAsync(id);
            return Ok(model);
        }
    }
}
