using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.Images.Command
{
    public class DeleteImageHandler<T>(AppDbContext context, IWebHostEnvironment webHostEnvironment) 
        : Handler(context) where T : Image
    {
        public virtual async Task<T> HandlerAsync(Guid id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null)
            {
                throw new Exception("Cannot find image.");
            }
            _context.Set<T>().Remove(entity);
            var localFilePath = Path.Combine(webHostEnvironment.ContentRootPath, "Images", $"{entity.Id}{entity.FileExtension}");
            try
            {
                // Check if file exists with its full path
                if (File.Exists(localFilePath))
                {
                    // If file found, delete it
                    File.Delete(localFilePath);
                    await _context.SaveChangesAsync();
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
            return entity;
        }
    }
}
