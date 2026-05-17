using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using TicketsSystem.Api.Filters;
using TicketsSystem.Api.Hubs;
using TicketsSystem.Api.Services;
using TicketsSystem.Core.DTOs.TicketsDTO;
using TicketsSystem.Core.DTOs.UserDTO;
using TicketsSystem.Core.Interfaces;
using TicketsSystem.Core.Services;
using TicketsSystem.Core.Validations.TicketsValidations;
using TicketsSystem.Core.Validations.UserValidations;
using TicketsSystem.Data;
using TicketsSystem.Data.Repositories;
using TicketsSystem.Data.Storage;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;
using Microsoft.Azure.SignalR;
using Supabase.Storage;

var builder = WebApplication.CreateBuilder(args);

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
// Add services to the container.
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});
// Swagger config using 8.1.4.
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese su token JWT directamente: "
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});
// Hash passwords
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
// Access to HttpContext
builder.Services.AddHttpContextAccessor();
// JWT Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        RoleClaimType = ClaimTypes.Role
    };

    // DEBUG: Enable detailed errors
    options.IncludeErrorDetails = true;

    // DEBUG: Verify Config Values
    var issuer = builder.Configuration["Jwt:Issuer"];
    var audience = builder.Configuration["Jwt:Audience"];
    var key = builder.Configuration["Jwt:Key"];

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Read JWT from the HttpOnly cookie (browser requests)
            var cookieToken = context.Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(cookieToken))
                context.Token = cookieToken;

            // Read JWT from query string for SignalR connections
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ticketHub"))
                context.Token = accessToken;

            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var tokenLen = context.Token?.Length ?? 0;
            var tokenPreview = tokenLen > 10 ? context.Token?.Substring(0, 10) : context.Token;
            logger.LogDebug("JWT token received (Len={TokenLength}): '{TokenPreview}...'", tokenLen, tokenPreview);

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "JWT authentication failed: {ExceptionType} - {Message}",
                context.Exception.GetType().Name, context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var claims = string.Join(", ", context.Principal.Claims.Select(c => $"{c.Type}={c.Value}"));
            logger.LogDebug("JWT token validated for user '{UserName}'. Claims: {Claims}",
                context.Principal.Identity.Name, claims);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            if (context.AuthenticateFailure != null)
            {
                logger.LogWarning("JWT challenge triggered. Error: '{Error}', Description: '{Description}', Failure: {FailureMessage}",
                    context.Error, context.ErrorDescription, context.AuthenticateFailure.Message);
            }
            else
            {
                logger.LogWarning("JWT challenge triggered (silent failure). Error: '{Error}', Description: '{Description}'",
                    context.Error, context.ErrorDescription);
            }
            return Task.CompletedTask;
        }
    };
});
// Supabase Storage
builder.Services.AddSingleton(sp =>
{
    var storageUrl = builder.Configuration["Supabase:Url"];
    var key = builder.Configuration["Supabase:Key"];

    if (string.IsNullOrWhiteSpace(storageUrl) || string.IsNullOrWhiteSpace(key))
        throw new InvalidOperationException("Supabase configuration is missing. Set Supabase:Url and Supabase:Key in appsettings.");

    return new Client(storageUrl, new Dictionary<string, string>
    {
        { "Authorization", $"Bearer {key}" }
    });
});
// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();

        policy.WithOrigins("https://zealous-sea-0a120c810.7.azurestaticapps.net")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});
// Database conexion
builder.Services.AddDbContext<SystemTicketsContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));
// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITicketsRepository, TicketsRepository>();
builder.Services.AddScoped<ITicketCommentsRepository, TicketCommentsRepository>();
builder.Services.AddScoped<ITicketsHistoryRepository, TicketsHistoryRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ITicketAttachmentRepository, TicketAttachmentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITicketsService, TicketsService>();
builder.Services.AddScoped<IGetUserRole, GetUserRoleService>();
builder.Services.AddScoped<ITicketCommetsService, TicketCommentsService>();
builder.Services.AddScoped<ITicketHistoryService, TicketHistoryService>();
builder.Services.AddScoped<ITicketHubService, TicketHubService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITicketAttachmentService, TicketAttachmentService>();
builder.Services.AddScoped<IFileStorageProvider, SupabaseStorageProvider>();
builder.Services.AddScoped<IStorageService, StorageService>();
// Validations 
builder.Services.AddTransient<IValidator<UserCreateDto>, UserCreateValidator>();
builder.Services.AddTransient<IValidator<UserUpdateDto>, UserUpdateValidator>();
builder.Services.AddTransient<UserPasswordValidator>();
builder.Services.AddTransient<IValidator<LoginRequest>, LoginRequestValidation>();
builder.Services.AddTransient<IValidator<TicketsCreateDto>, TicketsCreateValidator>();
builder.Services.AddTransient<IValidator<TicketsUpdateDto>, TicketsUpdateValidator>();
builder.Services.AddTransient<IValidator<GetAllTicketsFilterDto>, TicketsFilterValidation>();

if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSignalR();
}
else
{
    builder.Services.AddSignalR().AddAzureSignalR(builder.Configuration["Azure:SignalR:ConnectionString"]!);
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SystemTicketsContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }

    if (!dbContext.Users.Any())
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var adminUser = new User
        {
            UserId = Guid.NewGuid(),
            FullName = "Administrator",
            Email = "admin@example.com",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin123!");
        dbContext.Users.Add(adminUser);
        dbContext.SaveChanges();
        logger.LogInformation("Default admin user created successfully. It is recommended to change the email and password.");
    }

    if (!dbContext.TicketStatuses.Any())
    {
        var openStatus = new TicketStatus
        {
            Name = "Open"
        };

        var inProgressStatus = new TicketStatus
        {
            Name = "In Progress"
        };

        var onHoldStatus = new TicketStatus
        {
            Name = "On Hold"
        };

        var closedStatus = new TicketStatus
        {
            Name = "Closed"
        };

        var reopenedStatus = new TicketStatus
        {
            Name = "Reopened"
        };

        dbContext.TicketStatuses.Add(openStatus);
        dbContext.TicketStatuses.Add(inProgressStatus);
        dbContext.TicketStatuses.Add(onHoldStatus);
        dbContext.TicketStatuses.Add(closedStatus);
        dbContext.TicketStatuses.Add(reopenedStatus);
        dbContext.SaveChanges();
        logger.LogInformation("Default ticket statuses created successfully.");
    }

    if (!dbContext.TicketPriorities.Any())
    {
        var lowPriority = new TicketPriority
        {
            Name = "Low",
            Level = 1
        };

        var mediumPriority = new TicketPriority
        {
            Name = "Medium",
            Level = 2
        };

        var highPriority = new TicketPriority
        {
            Name = "High",
            Level = 3
        };

        var criticalPriority = new TicketPriority
        {
            Name = "Critical",
            Level = 4
        };

        dbContext.TicketPriorities.Add(lowPriority);
        dbContext.TicketPriorities.Add(mediumPriority);
        dbContext.TicketPriorities.Add(highPriority);
        dbContext.TicketPriorities.Add(criticalPriority);
        dbContext.SaveChanges();
        logger.LogInformation("Default ticket priorities created successfully.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<TicketHub>("/ticketHub");

app.MapControllers();

app.Run();

public partial class Program
{
}
