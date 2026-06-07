using Microsoft.AspNetCore.Mvc;
using VehicleTrackerApi.Dtos;
using VehicleTrackerApi.Models;
using VehicleTrackerApi.Services;
using LoginRequest = VehicleTrackerApi.Dtos.LoginRequest;
using RegisterRequest = VehicleTrackerApi.Dtos.RegisterRequest;

namespace VehicleTrackerApi.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController(AuthService authService, ILogger<AuthController> logger) : ControllerBase
  {
    private readonly AuthService _authService = authService;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("register")]
    public async Task<IActionResult> PostUser([FromBody] RegisterRequest registerRequest)
    {
      if (registerRequest == null)
      {
        _logger.LogWarning("User payload was null.");
        return BadRequest("User data is required.");
      }
 
      User user = new()
      {
        Email = registerRequest.Email,
        Username = registerRequest.Username,
        PasswordHash = registerRequest.Password,
      };

      Result result = await _authService.AddUser(user);

      return Ok(new { result });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
      User? user = await _authService.Authenticate(loginRequest);

      if (user == null) return Unauthorized();

      string token = _authService.CreateToken(user);
      return Ok(new { token });
    }
  }
}