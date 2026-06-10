using Microsoft.AspNetCore.Mvc;
using VehicleTrackerApi.Dtos;
using VehicleTrackerApi.Services;

namespace VehicleTrackerApi.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController(AuthService authService) : ControllerBase
  {
    [HttpPost("register")]
    public async Task<IActionResult> PostUser([FromBody] RegisterRequest registerRequest)
    {
      var newUser = new CreateUserInput(
        registerRequest.Username.Trim(),
        registerRequest.Email.Trim(),
        registerRequest.Password
      );

      var result = await authService.AddUser(newUser);

      if (!result.Success)
        return BadRequest(result.Message);

      return Ok(result.Data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
      var user = await authService.Authenticate(loginRequest);

      if (!user.Success)
        return Unauthorized("Invalid username or password");

      string token = authService.CreateToken(user.Data!);
      return Ok(new { token });
    }
  }
}