using WebAPI.Features.Auth;
using WebAPI.Features.Auth.Command;
using WebAPI.Features.Auth.Query;
using WebAPI.Features.Chat.ChatMessage.Command;
using WebAPI.Features.Chat.ChatMessage.Query;
using WebAPI.Features.Chat.ChatRoom.Command;
using WebAPI.Features.Chat.ChatRoom.Query;
using WebAPI.Features.Email;
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
            services.AddScoped<SendVerificationEmailHandler>();

            services.AddScoped<CreateMessagesHandler>();
            services.AddScoped<DeleteMessagesHandler>();
            services.AddScoped<GetMessagesByRoomIdHandler>();

            services.AddScoped<CreateRoomHandler>();
            services.AddScoped<DeleteRoomHandler>();
            services.AddScoped<GetAllRoomsHandler>();
            
            return services;
        }
    }
}
