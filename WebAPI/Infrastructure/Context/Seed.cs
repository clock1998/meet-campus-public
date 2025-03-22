using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAPI.Features.Auth;
using WebAPI.Features.CMS.Domains;
using WebAPI.Features.UserProfiles;

namespace WebAPI.Infrastructure.Context
{
    public static class Seed
    {
        public static void Run(ModelBuilder builder)
        {
            var adminRoleId = Guid.Parse(("B105A482-CF06-4D39-BAD3-F7D4B152C798"));
            var studentRoleId = Guid.Parse("C9F776DD-26B4-495A-97B9-0F42BAE68CEB");

            var roles = new List<ApplicationRole> {
                 new ApplicationRole(){
                    Id=adminRoleId,
                    Name="Admin",
                    NormalizedName="Admin".ToUpper()
                },
                new ApplicationRole(){
                    Id=studentRoleId,
                    Name="Student",
                    NormalizedName="Student".ToUpper()
                }
            };

            builder.Entity<ApplicationRole>().HasData(roles);

            //var genderId = Guid.NewGuid();
            //var genders = new List<Gender> {
            //     new Gender(){
            //         Id = genderId,
            //         Name = "Male"
            //    },
            //    new Gender(){
            //        Id = Guid.NewGuid(),
            //        Name = "Female"
            //    },
            //    new Gender(){
            //        Id = Guid.NewGuid(),
            //        Name = "Other"
            //    },
            //};
            //builder.Entity<Gender>().HasData(genders);

            var userProfileId = Guid.Parse("82A7DCD7-88FE-4E78-8A90-C345E812599C");
            builder.Entity<UserProfile>().HasData(new UserProfile()
            {
                Id = userProfileId,
                FirstName = "Admin",
                LastName = "Admin",
                //GenderId = genderId
            });

            var userId = Guid.Parse("625D90E8-6E56-47FA-8BD4-EA17C8BE3D79");
            var applicationUser = new ApplicationUser()
            {
                Id = userId,
                UserName = "example@email.com",
                NormalizedUserName = "example@email.com",
                Email = "example@email.com",
                NormalizedEmail = "example@email.com",
                EmailConfirmed = true,
                SecurityStamp = "5MF37NW5IFTFHSYQX2YU47KYBFFZTQCY",
                ConcurrencyStamp = "497d71e3-d852-4c33-ba8a-075a56a54d7e",
                UserProfileId = userProfileId,
                PasswordHash = "AQAAAAIAAYagAAAAEMk6Y3Ie5t0YutW/YqMNE6MgH7oDg4a6OtAisNfZ8BSfTcmnVfr9ZW1PatTJWTIhVg=="
            };
            var hasher = new PasswordHasher<ApplicationUser>();
            //applicationUser.PasswordHash = hasher.HashPassword(applicationUser, "YourPassword"); 

            builder.Entity<ApplicationUser>().HasData(applicationUser);

            builder.Entity<ApplicationUserRole>().HasData(new ApplicationUserRole()
            {
                UserId = userId,
                RoleId = adminRoleId
            });

            builder.Entity<Domain>().HasData(new Domain()
            {
                Id = Guid.Parse("E03F2F1C-C63C-40F3-A3A0-3A97DB6315AC"),
                Record = "mcgill.ca"
            });
        }
    }
}
