

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.WebAPI.Repositories.Core;
using Template.WebAPI.Repositories.Inerface;
using WebAPI.Models.Image;
using WebAPI.Features.Images;
using WebAPI.Features.Users;
using WebAPI.Infrastructure.Helper;

namespace Template.WebAPI.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class UserProfileController : Controller
    {
        private readonly IRepository<UserProfile> respository;
        private readonly IImageRepository<ProfileImage> profileImageRepository;

        public UserProfileController(IRepository<UserProfile> respository, IImageRepository<ProfileImage> profileImageRepository)
        {
            this.respository = respository;
            this.profileImageRepository = profileImageRepository;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAll([FromQuery] CourseParameters queryParameters)
        //{
        //    var courses = await respository.GetAllByConditionAsync(queryParameters);
        //    Response.Headers.Add("X-Pagination", courses.GeneratePagedMeta());
        //    //courses.Include(typeof(Semester).ToString()).Where
        //    return Ok(courses);
        //}
        public class UserProfileDTO
        {
            public required string FirstName { get; set; }
            public required string LastName { get; set; }
            public DateTime? DateOfBirth { get; set; }

            public Guid GenderId { get; set; }
            public Guid? SexsualityId { get; set; }

            public Guid? DatingGenderId { get; set; }

            public Guid? EthnicityId { get; set; }

            public Guid? ProgramId { get; set; }
        }
        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var model = await respository.GetAsync(id);

            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UserProfileDTO userProfile)
        {
            //var model = await respository.GetAsync(id);
            //if (model == null)
            //{
            //    return NotFound();
            //}
            //model.FirstName=userProfile.FirstName; model.LastName=userProfile.LastName;
            //model.DateOfBirth=userProfile.DateOfBirth;
            //var updatedModel = await respository.UpdateAsync(id, model);

            return Ok();
        }

        [HttpPost, Route("Upload")]
        public async Task<IActionResult> Upload([FromForm] ProfileImageDTO imageUploadDTO)
        {
            var modelState = await FileHelpers.CheckFileAsync(imageUploadDTO.Images.Select(n=> n.File).ToArray(), ModelState, new string[] { ".jpg", ".png", ".jpeg" }, 52428800);
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
            foreach(var image in images)
            {
               await profileImageRepository.CreateAsync(image);
            }
            
            return Ok(images);
        }

        [HttpDelete, Route("DeleteProfileImage/{id:Guid}")]
        public async Task<IActionResult> DeleteProfileImage([FromRoute] Guid id)
        {
            var model = await profileImageRepository.Delete(id);
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }
    }
}
