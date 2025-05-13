using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using WebAPI.Features.Auth;
using WebAPI.Features.Chat.ChatMessage;
using WebAPI.Features.Chat.ChatRoom;
using WebAPI.Features.CMS.Domains;
using WebAPI.Features.CMS.Emails;
using WebAPI.Features.Courses;
using WebAPI.Features.Images;
using WebAPI.Features.Semesters;
using WebAPI.Features.UserProfiles;

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
        public DbSet< Room> Rooms { get; set; }
        
        public DbSet<RoomUser> RoomUsers { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<ProfileImage> ProfileImages { get; set; }
        public DbSet<Domain> Domains { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            OnModelCreatingAuth(builder);
            Seed.Run(builder);
            builder.Entity<ApplicationUser>()
                .HasMany(e => e.Courses)
                .WithMany(e => e.ApplicationUsers)
                .UsingEntity<CourseUser>(
                l => l.HasOne<Course>().WithMany().HasForeignKey(e => e.CourseId),
                r => r.HasOne<ApplicationUser>().WithMany().HasForeignKey(e => e.UserId));

            builder.Entity<ApplicationUser>()
                .HasMany(e => e.Rooms)
                .WithMany(e => e.ApplicationUsers)
                .UsingEntity<RoomUser>(
                l => l.HasOne<Room>(n=>n.Room).WithMany().HasForeignKey(e => e.RoomId),
                r => r.HasOne<ApplicationUser>(n=>n.User).WithMany().HasForeignKey(e => e.UserId));

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
    }
}
