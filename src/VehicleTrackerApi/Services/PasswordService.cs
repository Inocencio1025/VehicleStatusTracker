using Microsoft.AspNetCore.Identity;
using VehicleTrackerApi.Models;

namespace VehicleTrackerApi.Services
{
  public class PasswordService
  {
    private readonly PasswordHasher<User> _hasher = new();
    public string Hash(User user, string pw)
    {
      return _hasher.HashPassword(user, pw);
    }

    public bool Verify(User user, string enteredPassword)
    {
      var result = _hasher.VerifyHashedPassword(
          user,
          user.Password,
          enteredPassword
      );

      if (result == PasswordVerificationResult.Failed)
        return false;

      return true;
    }
  }
}