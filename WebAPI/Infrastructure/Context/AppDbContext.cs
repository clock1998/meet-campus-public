using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using WebAPI.Features.CMS.Domains;
using WebAPI.Features.CMS.Emails;
using WebAPI.Features.Courses;
using WebAPI.Features.Images;
using WebAPI.Features.Messages;
using WebAPI.Features.Semesters;
using WebAPI.Features.Users;
using WebAPI.Infrastructure;

namespace WebAPI.Infrastructure.Context
{
    public class AppDbContext : IdentityDbContext<
        ApplicationUser, ApplicationRole, Guid,
        ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin,
        ApplicationRoleClaim, ApplicationUserToken>
    {
        public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<ProfileImage> ProfileImages { get; set; }
        public DbSet<Domain> Domains { get; set; }
        public DbSet<Email> Emails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            OnModelCreatingAuth(builder);
            Seed(builder);
            builder.Entity<ApplicationUser>()
                .HasMany(e => e.Courses)
                .WithMany(e => e.ApplicationUsers)
                .UsingEntity<CourseUser>(
                l => l.HasOne<Course>().WithMany().HasForeignKey(e => e.CourseId),
                r => r.HasOne<ApplicationUser>().WithMany().HasForeignKey(e => e.UserId));

            builder.Entity<Email>()
            .HasIndex(b => b.EmailAdress)
            .IsUnique();

        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => x.Entity is Entity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((Entity)entity.Entity).Created = DateTime.UtcNow;
                }

                ((Entity)entity.Entity).Updated = DateTime.UtcNow;
            }
        }

        public void OnModelCreatingAuth(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>(b =>
            {
                // Each User can have many UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne(e => e.User)
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Each User can have many UserLogins
                b.HasMany(e => e.Logins)
                    .WithOne(e => e.User)
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Each User can have many UserTokens
                b.HasMany(e => e.Tokens)
                    .WithOne(e => e.User)
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Each User can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.User)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            builder.Entity<ApplicationRole>(b =>
            {
                // Each Role can have many entries in the UserRole join table
                b.HasMany(e => e.UserRoles)
                    .WithOne(e => e.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                // Each Role can have many associated RoleClaims
                b.HasMany(e => e.RoleClaims)
                    .WithOne(e => e.Role)
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
            });


        }
        public void Seed(ModelBuilder builder)
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
