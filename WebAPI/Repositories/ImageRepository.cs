using System.Net;
using Template.WebAPI.Repositories.Core;
using Template.WebAPI.Repositories.Inerface;
using WebAPI.Infrastructure.Context;
using WebAPI.Features.Images;

namespace Template.WebAPI.Repositories
{
    public class ImageRepository<T> : Repository<T>, IImageRepository<T> where T : Image
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ImageRepository(AppDbContext context, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }
        public override async Task<T> CreateAsync(T image)
        {
            var trustedFileNameForDisplay = WebUtility.HtmlEncode(Path.GetFileNameWithoutExtension(image.File.FileName));
            if (!string.IsNullOrEmpty(image.FileName))
            {
                trustedFileNameForDisplay = WebUtility.HtmlEncode(image.FileName);
            }
            image.FileName = trustedFileNameForDisplay;
            image.FilePath = $"";
            // create the image in the data base first to get the id.
            await _dbContext.AddAsync(image);
            await _dbContext.SaveChangesAsync();

            var request = httpContextAccessor.HttpContext?.Request;
            image.FilePath = $"{request?.Scheme}://{request?.Host}{request?.PathBase}/Images/{image.Id}{image.FileExtension}";
            // create the file path using the generated id to avoid duplicate names.
            await _dbContext.SaveChangesAsync();
            var localFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "Images", $"{image.Id}{image.FileExtension}");
            using (var fileStream = new FileStream(localFilePath, FileMode.Create))
            {
                await image.File.CopyToAsync(fileStream);
            }
            return image;
        }

        public override async Task<T> Delete(Guid id)
        {
            var model = await base.Delete(id);
            var localFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "Images", $"{model.Id}{model.FileExtension}");
            try
            {
                // Check if file exists with its full path
                if (File.Exists(localFilePath))
                {
                    // If file found, delete it
                    File.Delete(localFilePath);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    throw new InvalidOperationException("File path does not exist.");
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return model;
        }
    }
}
