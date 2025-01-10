using WebAPI.Features.Auth;
using WebAPI.Features.Semesters;
using WebAPI.Infrastructure;

namespace WebAPI.Features.Courses
{
    // do not initiate object in the model
    public class Course : Entity
    {
        public int Section { set; get; }
        public string Name { set; get; } = string.Empty;
        public Guid SemesterId { set; get; }
        public virtual Semester Semester { set; get; } = null!;
        public virtual List<ApplicationUser> ApplicationUsers { get; set; } = null!;
    }
}
