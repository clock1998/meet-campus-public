using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using WebAPI.Features.Images;
using WebAPI.Infrastructure;
namespace WebAPI.Features.UserProfiles
{
    public class UserProfile : Entity
    {
        [NotMapped, DisplayName]
        public string DisplayName => FirstName + " " + LastName;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }

        //public Gender Gender { get; set; } = null!;

        public virtual List<ProfileImage> ProfileImages { get; set; } = null!;
    }
}
