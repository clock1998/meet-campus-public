using WebAPI.Infrastructure.Context;

namespace WebAPI.Features
{
    public abstract class Handler
    {
        protected readonly AppDbContext _context;
        public Handler(AppDbContext context)
        {
            _context = context;
        }
    }
}
