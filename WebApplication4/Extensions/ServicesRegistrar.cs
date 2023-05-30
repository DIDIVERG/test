using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApplication4.Database;
using WebApplication4.Database.Helpers;
using WebApplication4.Database.Models;
using WebApplication4.Middleware;
using WebApplication4.Profiles;
using WebApplication4.Services.JwtTokenGeneratorService;

namespace WebApplication4.Extensions;

public static class ServicesRegistrar
{

    public static IServiceCollection AddServices(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddDbContext<UserContext>(o =>
        {
            o.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            o.UseSnakeCaseNamingConvention();
        });
        collection.AddScoped<EnsureDatabaseCreatedMiddleware>();
        collection.AddScoped<SetSwaggerDefaultHeaderValueMiddleware>();
        collection.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        collection.AddScoped<IDatabaseHelper, DatabaseHelper>();
        collection.AddAutoMapper(typeof(UserProfile));
        collection.AddAuthorization(options =>
        {
            options.AddPolicy("OnlyAdmin", policy =>
            {
                policy.RequireRole("admin"); 
                policy.RequireClaim("revoked", "false"); 
            });
            options.AddPolicy("OnlyUser", policy =>
            {
                policy.RequireRole("user"); 
                policy.RequireClaim("revoked", "false"); 
            });
            options.AddPolicy("All", policy =>
            {
                policy.RequireClaim("revoked", "false"); 
            });
        });
        collection.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"], 
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey
                        (Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"] ?? string.Empty))
                };
            });
        collection.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
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
                    Array.Empty<string>()
                }
            });
        });
    


        return collection;
    }
}