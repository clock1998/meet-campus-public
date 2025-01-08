using WebAPI.Infrastructure;

namespace WebAPI.Features.Semesters
{
    public class Semester : Entity
    {
        public string Name { get; set; } = string.Empty;
        public int Year { get; set; }
    }
}
