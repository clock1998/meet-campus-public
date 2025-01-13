using Microsoft.AspNetCore.Mvc;
using WebAPI.Features.Images;
using WebAPI.Features.Images.Command;
using WebAPI.Infrastructure.Context;
using WebAPI.Infrastructure.Helper;

namespace WebAPI.Features.UserProfiles.Command
{
    public record ProfileImageUploadRequest(ImageFile[] Images, string UserProfileId) : ImageUploadRequest(Images);
    public class UploadProfileImageHandler(AppDbContext context, HttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment)
        : UploadImageHandler<ProfileImage>(context, httpContextAccessor, webHostEnvironment)
    {
        public override async Task<ProfileImage> HandleAsync(ProfileImage image)
        {
            var newImage = await base.HandleAsync(image);
            newImage.UserProfileId = image.UserProfileId;
            await _context.SaveChangesAsync();
            return newImage;
        }
    }
}
