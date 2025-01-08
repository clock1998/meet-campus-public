using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Room
{
    public class RoomDTO
    {
        public string UserId { get; set; }
        public IFormFile Avatar { get; set; }
        public Guid CourseId { get; set; }
        public string Name { get; set; }
        public string RoomId { get; set; }
    }
}
