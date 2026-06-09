using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Dtos;
using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Services
{
  public class AuthService(
    VehicleTrackerContext context,
    ILogger<AuthService> logger,
    PasswordService passwordService,
    IConfiguration configuration)
  {
    public async Task<Result<User>> Authenticate(LoginRequest loginRequest)
    {
      var user = await context.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

      if (user == null) 
        return new Result<User>(false, "User does not exist", null);

      bool youGood = passwordService.Verify(user, loginRequest.Password);

      if (!youGood) 
        return new Result<User>(false, "Wrong Credentials", null);
      
      return new Result<User>(true, "Authenticated", user);
    }

    public async Task<Result<User>> AddUser(User user)
    {
      user.Password = passwordService.Hash(user, user.Password);

      // add validation check here

      context.Add(user);
      await context.SaveChangesAsync();

      logger.LogInformation("User added with ID: {UserId}.", user.Id);
      return new Result<User>(true, "User created successfully", user);
    }

    public string CreateToken(User user)
    {
      string? secretKey = configuration["Jwt:Key"];

      if (string.IsNullOrWhiteSpace(secretKey))
        throw new InvalidOperationException("JWT key is missing.");

      var claims = new[]
      {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
      };

      var key = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(secretKey)
      );

      var creds = new SigningCredentials(
          key,
          SecurityAlgorithms.HmacSha256
      );

      var token = new JwtSecurityToken(
          claims: claims,
          expires: DateTime.UtcNow.AddHours(24),
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);
    }
  }
}