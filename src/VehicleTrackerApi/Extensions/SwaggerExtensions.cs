using Microsoft.OpenApi.Models;

namespace VehicleTrackerApi.Extensions;

public static class SwaggerExtensions
{
  private const string SecurityScheme = "Bearer";
  public static IServiceCollection AddAppSwagger(this IServiceCollection services)
  {
    
    services.AddSwaggerGen(options =>
    {
      options.AddSecurityDefinition(SecurityScheme, new OpenApiSecurityScheme
      {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = SecurityScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token"
      });

      options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
          {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = SecurityScheme
                }
            },
            Array.Empty<string>()
          }
        });
    });

    return services;
  }
}