using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.Infrastructure.Context;

namespace WebAPI.Features.UserProfiles.Command
{
    public record UserProfileRequest(string FirstName, string LastName, DateTime? DateOfBirth, Guid GenderId, Guid? SexsualityId, Guid? DatingGenderId, Guid? EthnicityId, Guid? ProgramId);
    public class UpdateUserProfileHandler(AppDbContext context, HttpContextAccessor httpContextAccessor)
    {
        public async Task<UserProfile> HandleAsync(Guid id, UserProfileRequest request)
        {
            var identity = httpContextAccessor.HttpContext!.User.Identity as ClaimsIdentity;
            if(identity == null)
            {
                throw new NullReferenceException("ClaimsIdentity is null");
            }
            
            IEnumerable<Claim> claims = identity.Claims;
            var userId = claims.FirstOrDefault(n => n.Type.Equals(ClaimTypes.NameIdentifier));
            var user = await context.Users.FindAsync(userId?.Value);

            var entity = await context.UserProfiles.FindAsync(id);
            if(user?.UserProfileId != id)
            {
                throw new Exception("Wrong User profile id.");
            }
            if (entity == null)
            {
                throw new Exception("User Profile not found.");
            }
            entity.FirstName = request.FirstName;
            entity.LastName = request.LastName;
            entity.DateOfBirth = request.DateOfBirth;
            await context.SaveChangesAsync();
            return entity;
        }
    }
}
