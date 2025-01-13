using WebAPI.Features.Auth;
using WebAPI.Features.Auth.Command;
using WebAPI.Features.Auth.Query;
using WebAPI.Features.UserProfiles.Command;
using WebAPI.Features.Users.Command;
using WebAPI.Features.Users.Query;

namespace WebAPI
{
    public static class HandlerConfiguration
    {
        public static IServiceCollection AddHandlerConfiguration(this IServiceCollection services)
        {
            #region User
            services.AddScoped<CreateUserHandler>();
            services.AddScoped<DeleteUserHandler>();
            services.AddScoped<GetUserByIdHandler>();
            #endregion

            #region UserProfile
            services.AddScoped<UpdateUserProfileHandler>();
            services.AddScoped<UploadProfileImageHandler>();
            #endregion

            services.AddScoped<AuthHandler>();
            services.AddScoped<LoginHandler>();
            services.AddScoped<RegisterHandler>();
            services.AddScoped<RefreshHandler>();
            services.AddScoped<VerifyEmailHander>();
            return services;
        }
    }
}
