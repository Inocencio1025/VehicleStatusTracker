using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VehicleTrackerApi.Constants;

namespace VehicleTrackerApi.Extensions;

public static class AuthenticationExtensions
{
  public static IServiceCollection AddAppAuthentication(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    var jwtKey = configuration["Jwt:Key"];

    services
      .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = false,
          ValidateAudience = false,
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey!)
          )
        };

        options.Events = new JwtBearerEvents
        {
          OnMessageReceived = context =>
          {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken)
                    && path.StartsWithSegments(Routes.VehicleHub))
            {
              context.Token = accessToken;
            }

            return Task.CompletedTask;
          }
        };
      });

    return services;
  }
}