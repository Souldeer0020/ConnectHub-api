using Azure.Core;
using ConnectHub.Application.DTO_s.Common;
using ConnectHub.Application.Hubs;
using ConnectHub.Application.Interfaces;
using ConnectHub.Application.Settings;
using ConnectHub.Application.Specifications;
using ConnectHub.Hubs;
using ConnectHub.Infrastructure.Persistence;
using ConnectHub.Infrastructure.Repositories;
using ConnectHub.Infrastructure.Services;
using ConnectHub.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/connecthub.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
    .CreateLogger();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});
// ============================================================
// Controllers & API Explorer
// ============================================================

// Adds support for API controllers
builder.Services.AddControllers();

// Enables endpoint discovery for Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR();

// ============================================================
// Swagger Configuration with JWT Authentication Support
// ============================================================

builder.Services.AddSwaggerGen(options =>
{
    // Adds JWT Bearer authentication to Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,

        // Instructions shown inside Swagger
        Description = "Enter JWT token like: eyJhbGciOiJIUzI1NiIs..."
    });

    // Makes Swagger send JWT token automatically
    // when calling protected endpoints
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },

            Array.Empty<string>()
        }
    });
});


// ============================================================
// Database Configuration
// ============================================================

// Registers AppDbContext and connects SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// ============================================================
// JWT Settings Configuration
// ============================================================

// Binds JwtSettings class with JwtSettings section
// from appsettings.json
builder.Services.Configure<JWTSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.Configure<FileSettings>(
    builder.Configuration.GetSection("FileSettings"));

// ============================================================
// JWT Authentication Configuration
// ============================================================

// Reads JWT settings from configuration
var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JWTSettings>()!;

// Converts secret key string into byte array
var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);


// Registers JWT Authentication
builder.Services.AddAuthentication(options =>
{
    // Sets JWT as default authentication method
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(options =>
{
    // Token validation rules
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            // Validates token issuer
            ValidateIssuer = true,

            // Validates token audience
            ValidateAudience = true,

            // Validates token expiration
            ValidateLifetime = true,

            // Validates token signature
            ValidateIssuerSigningKey = true,

            // Expected issuer
            ValidIssuer = jwtSettings.Issuer,

            // Expected audience
            ValidAudience = jwtSettings.Audience,

            // Secret key used for signature validation
            IssuerSigningKey =
                new SymmetricSecurityKey(key),

            // Removes default 5-minute tolerance
            ClockSkew = TimeSpan.Zero
        };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            Console.WriteLine($"OnMessageReceived fired — path: {path}, token present: {!string.IsNullOrEmpty(accessToken)}");

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
                Console.WriteLine("Token set from query string ✅");
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7145", "http://localhost:5190")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // ← required for SignalR
    });
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = (actionContext) =>
    {
        var errors = actionContext.ModelState.Where(p => p.Value.Errors.Count() > 0)
                                             .SelectMany(p => p.Value.Errors)
                                             .Select(p => p.ErrorMessage)
                                             .ToArray();
        var apiResponse = new ApiValidationErrorResponse
        {
            Errors = errors
        };
        return new BadRequestObjectResult(apiResponse);
    }; 
});
// ============================================================
// Dependency Injection
// ============================================================

// Registers authentication service  // we allow dependecy injection for these classess
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped(typeof( Ispecification<>),typeof( BaseSpecification<>));
builder.Services.AddScoped(typeof( IUnitOfWork),typeof(UnitOfWork));
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<INotificationHubContext, NotificationHubContext>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(S =>
{
    var connection = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(connection);
});

// ============================================================
// Build Application
// ============================================================

var app = builder.Build();


// ============================================================
// Middleware Pipeline
// ============================================================

// Enables Swagger only in development
app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirects HTTP requests to HTTPS
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors();
// Enables authentication middleware
// MUST come before authorization
app.UseAuthentication();

// Enables authorization middleware
app.UseAuthorization();


// Maps controller endpoints

app.MapHub<NotificationHub>("/hubs/notifications");

app.MapControllers();


// Starts application
app.Run();