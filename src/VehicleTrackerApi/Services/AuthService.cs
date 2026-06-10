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

    public async Task<Result<UserDto>> AddUser(CreateUserInput request)
    {
      //-------Validation checks-------
      var existingUser = await context.Users
        .AnyAsync(u => u.Username == request.Username);
      
      if (existingUser)
        return new Result<UserDto>(false, "Username already exists", null);
      
      existingUser = await context.Users
        .AnyAsync(u => u.Email == request.Email);
      
      if (existingUser)
        return new Result<UserDto>(false, "Email already exists", null);

      bool hasUpper = request.Password.Any(char.IsUpper);
      bool hasLower = request.Password.Any(char.IsLower);
      bool hasDigit = request.Password.Any(char.IsDigit);

      if (request.Password.Length < 8)
        return new Result<UserDto>(false, "Password must be at least 8 characters.", null);

      if (!hasUpper || !hasLower || !hasDigit)
        return new Result<UserDto>(false, "Password requirements not met.", null);
      //-------End of validation checks----------

      
      var newUser = new User
      {
        Username = request.Username,
        Email = request.Email
      };
      newUser.Password = passwordService.Hash(newUser, request.Password);

      context.Add(newUser);
      await context.SaveChangesAsync();

      logger.LogInformation("User added with ID: {UserId}.", newUser.Id);
 
      return new Result<UserDto>(true, "User created successfully", 
        new UserDto(newUser.Username, newUser.Email));
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