using WebAPI.Features.Auth;

namespace WebAPI
{
    public static class HandlerConfiguration
    {
        public static IServiceCollection AddHandlerConfiguration(this IServiceCollection services)
        {
            services.AddScoped<AuthHandler>();
            services.AddScoped<LoginHandler>();
            services.AddScoped<RegisterHandler>();
            services.AddScoped<RefreshHandler>();
            services.AddScoped<VerifyEmailHander>();
            return services;
        }
    }
}
