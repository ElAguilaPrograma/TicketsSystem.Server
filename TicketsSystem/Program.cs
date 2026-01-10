using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using TicketsSystem.Core.Services;
using TicketsSystem.Core.Validations;
using TicketsSystem.Data.Repositories;
using TicketsSystem_Data;
using TicketsSystem_Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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
// Registrar el acceso al HttpContext
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
    Console.WriteLine($"[DEBUG] Startup Config - Issuer: '{issuer}', Audience: '{audience}', KeyLength: {key?.Length ?? 0}");

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine($"[FAIL] Authentication failed: {context.Exception.Message}");
            Console.WriteLine($"[FAIL] Exception Type: {context.Exception.GetType().Name}");
            Console.WriteLine("--------------------------------------------------------------");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine($"[SUCCESS] Token validated. User: {context.Principal.Identity.Name}");
            foreach (var claim in context.Principal.Claims)
            {
                Console.WriteLine($"  Claim: {claim.Type} - {claim.Value}");
            }
            Console.WriteLine("--------------------------------------------------------------");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine($"[CHALLENGE] OnChallenge Triggered.");
            Console.WriteLine($"[CHALLENGE] Error: '{context.Error}', Desc: '{context.ErrorDescription}'");
            if (context.AuthenticateFailure != null)
            {
                 Console.WriteLine($"[CHALLENGE] AuthenticateFailure: {context.AuthenticateFailure.Message}");
                 Console.WriteLine($"[CHALLENGE] Failure Trace: {context.AuthenticateFailure.StackTrace}");
            }
            else 
            {
                 Console.WriteLine("[CHALLENGE] No AuthenticateFailure exception found (Silent failure?).");
            }
            Console.WriteLine("--------------------------------------------------------------");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            Console.WriteLine("--------------------------------------------------------------");
            var tokenLen = context.Token?.Length ?? 0;
            var tokenPreview = tokenLen > 10 ? context.Token?.Substring(0, 10) : context.Token;
            Console.WriteLine($"[RECEIVED] Token (Len={tokenLen}): '{tokenPreview}...'");
            Console.WriteLine("--------------------------------------------------------------");
            return Task.CompletedTask;
        }
    };
});
// Database conexion
builder.Services.AddDbContext<SystemTicketsContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITicketsRepository, TicketsRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITicketsService, TicketsService>();
builder.Services.AddScoped<IGetUserRole, GetUserRoleService>();

// Validations
builder.Services.AddScoped<UserDTOValidator, UserDTOValidator>();
builder.Services.AddScoped<LoginRequestValidation, LoginRequestValidation>();
builder.Services.AddScoped<TicketsDTOValidator, TicketsDTOValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
