

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VehicleTrackerApi.Data;
using VehicleTrackerApi.Dtos;
using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Services
{
  public class AuthService
  {

    private readonly ILogger<AuthService> _logger;
    private readonly VehicleTrackerContext _context;
    private readonly PasswordService _passwordService;
    private readonly IConfiguration _configuration;

    public AuthService(
      VehicleTrackerContext context,
      ILogger<AuthService> logger,
      PasswordService passwordService,
      IConfiguration configuration)
    {
      _context = context;
      _logger = logger;
      _passwordService = passwordService;
      _configuration = configuration;
    }

    public async Task<User?> Authenticate(LoginRequest loginRequest)
    {
      var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

      if (user == null) return null;

      bool youGood = _passwordService.Verify(user, loginRequest.Password);

      if (!youGood) return null;
      
      return user;
    }

    public async Task<Result> AddUser(User user)
    {
      user.PasswordHash = _passwordService.Hash(user, user.PasswordHash);

      // add validation check here

      _context.Add(user);
      await _context.SaveChangesAsync();

      _logger.LogInformation("User added with ID: {UserId}.", user.Id);
      return new Result(true, "User created successfully");
    }

    public string CreateToken(User user)
    {
      /* 
        ai told me ! is risky and may cause runtime crash.
        reminder to address this eventually ig
      */
      string secretkey = _configuration["Jwt:Key"]!;

      var claims = new[]
      {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
      };

      var key = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(secretkey)
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