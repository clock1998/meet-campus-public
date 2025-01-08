using Template.WebAPI.Repositories.Core;
using WebAPI.Features.Images;

namespace Template.WebAPI.Repositories.Inerface
{
    public interface IImageRepository<T>: IRepository<T> where T : Image
    {
        //Place to add non standard functions.
    }
}
