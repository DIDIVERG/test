using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        collection.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        collection.AddScoped<IDatabaseHelper, DatabaseHelper>();
        collection.AddAutoMapper(typeof(UserProfile));
        collection.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Description = "OAuth2 password",
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Password = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri("/api/auth/login", UriKind.Relative), 
                        Scopes = new Dictionary<string, string>
                        {
                            { "values:read", "Read access to protected resources" },
                            { "values:write", "Write access to protected resources" }
                        }
                    }
                }
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });


        return collection;
    }
}