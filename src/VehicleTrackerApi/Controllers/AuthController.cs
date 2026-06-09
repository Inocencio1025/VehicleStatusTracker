using Microsoft.AspNetCore.Mvc;
using VehicleTrackerApi.Dtos;
using VehicleTrackerApi.Models;
using VehicleTrackerApi.Services;

namespace VehicleTrackerApi.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController(
    AuthService authService, 
    ILogger<AuthController> logger) : ControllerBase
  {
    [HttpPost("register")]
    public async Task<IActionResult> PostUser([FromBody] RegisterRequest registerRequest)
    {
      if (registerRequest == null)
      {
        logger.LogWarning("User payload was null.");
        return BadRequest("User data is required.");
      }
 
      User user = new()
      {
        Email = registerRequest.Email,
        Username = registerRequest.Username,
        Password = registerRequest.Password,
      };

      var result = await authService.AddUser(user);

      if (!result.Success)
        return BadRequest(result.Message);

      return Ok(result.Data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
      var user = await authService.Authenticate(loginRequest);
 
      if (!user.Success) 
        return Unauthorized(user.Message);

      string token = authService.CreateToken(user.Data!);
      return Ok(new { token });
    }
  }
}