namespace VehicleTrackerApi.Extensions;

public static class CorsExtensions
{
  private const string PolicyName = "CorsPolicy";

  public static IServiceCollection AddAppCors(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    var allowedOrigins = configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    services.AddCors(options =>
    {
      options.AddPolicy(PolicyName, policy =>
          {
          policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
        });
    });

    return services;
  }

  public static IApplicationBuilder UseAppCors(this IApplicationBuilder app)
  {
    app.UseCors(PolicyName);
    return app;
  }
}