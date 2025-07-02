using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using EmailService;
using System.Security.Cryptography;
using System.Text.Json;
using Template.WebAPI.HostedService;
using WebAPI.Infrastructure.Context;
using FluentValidation;
using Microsoft.AspNetCore.OData;
using WebAPI.Infrastructure.Exceptions;
using WebAPI;
using System.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using WebAPI.Features.Chat;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

#region Load private key for JWT
var rsaKey = RSA.Create();
if (!File.Exists("./key"))
{
    var privateKey = rsaKey.ExportRSAPrivateKey();
    File.WriteAllBytes("key", privateKey);
    Log.Information("New RSA Key created");
}
rsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);
#endregion

#region Load multiple appsettings.json files
//builder.Configuration
//    .SetBasePath(Directory.GetCurrentDirectory())
//    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
//    .AddEnvironmentVariables();
#endregion

builder.Services.
    AddProblemDetails(o =>
    {
        o.CustomizeProblemDetails = context =>
        {
            Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
            context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
            //context.ProblemDetails.Extensions.Add("traceId", activity?.Id);
        };
    })
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddHttpContextAccessor()
    .AddControllers().AddJsonOptions(ops => {
        ops.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        ops.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    })
    .AddOData(ops => ops.Select().Filter().Count().OrderBy().Expand());

#region Email
var emailConfig = config
        .GetSection("EmailConfiguration")
        .Get<EmailConfiguration>();
if (emailConfig != null) { 
    builder.Services.AddSingleton(emailConfig);
}
builder.Services.AddScoped<IEmailSender, EmailSender>();
#endregion

#region Serilog
Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();
builder.Services.AddSerilog();
#endregion

builder.Services.AddSwaggerConfiguration();

builder.Services.AddReverseProxy().LoadFromConfig(config.GetSection("ReverseProxy"));
builder.Services.AddDbContext<AppDbContext>(options => options.UseLazyLoadingProxies().UseNpgsql(config.GetConnectionString("DatabaseConnectionString")));

#region Repository Injection
builder.Services.AddHandlerConfiguration();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddSignalR(n => { n.EnableDetailedErrors = true; }).AddJsonProtocol(x => x.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
#endregion

#region Hosted Service
builder.Services.AddHostedService<MatchmakingService>();
#endregion

builder.Services.AddAuthConfiguration(config, rsaKey);

builder.Services.AddCors(options =>
{
   options.AddPolicy("LocalhostPolicy", policy =>
   {
       policy.WithOrigins("http://localhost:8081")
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials();
   });
});

try
{
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    //app.MapHub<WebRTCHub>("/hubs/chat");
    //app.MapHub<CloudflareHub>("/hubs/cloudflare");
    app.MapHub<ChatHub>("/hubs/chat");
    
    // Define the path to the Images directory
    var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "Images");

    // Check if the directory exists, if not, create it
    if (!Directory.Exists(imagesPath))
    {
        Directory.CreateDirectory(imagesPath);
    }
    app.UseStaticFiles(
        new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Images")),
            RequestPath = "/Images"
        });

    app.UseExceptionHandler();

    app.UseCors("LocalhostPolicy");

    app.MapControllers();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapReverseProxy();
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design") // see https://github.com/dotnet/efcore/issues/29923
{
    Log.Fatal(ex, "Unhandled exception. App start failed.");
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program { }