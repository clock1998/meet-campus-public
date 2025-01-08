using WebAPI.Features.Images;
using WebAPI.Infrastructure.Context;

namespace Template.WebAPI.Repositories
{
    public class ProfileImageRepository : ImageRepository<ProfileImage>
    {
        public ProfileImageRepository(AppDbContext context, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context, webHostEnvironment, httpContextAccessor)
        {
        }
        public override async Task<ProfileImage> CreateAsync(ProfileImage image)
        {
            var newImage = await base.CreateAsync(image);
            newImage.UserProfileId = image.UserProfileId;
            await _dbContext.SaveChangesAsync();
            return newImage;
        }
    }
}
