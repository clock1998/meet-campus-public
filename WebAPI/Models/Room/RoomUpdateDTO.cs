using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Room
{
    public class RoomUpdateDTO
    {
        public IFormFile Avatar { get; set; }
        public string Name { get; set; }
    }
}
