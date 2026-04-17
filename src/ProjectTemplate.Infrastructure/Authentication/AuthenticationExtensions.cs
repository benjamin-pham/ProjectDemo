using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ProjectTemplate.Infrastructure.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationSchemes(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Authentication:Jwt");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt["Issuer"],
                    ValidAudience = jwt["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt["SecretKey"]!))
                };
            })
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationHandler.SchemeName, _ => { })
            .AddScheme<AuthenticationSchemeOptions, ScalarBasicAuthenticationHandler>("ScalarBasicAuth", null);

        services.AddAuthorization(options =>
        {
            //scalar user
            options.AddPolicy("ScalarBasic", policy =>
            {
                policy.AddAuthenticationSchemes("ScalarBasicAuth");
                policy.RequireAuthenticatedUser();
            });
        });

        return services;
    }
}
