using BCrypt.Net;
using CarShowRoom.DAL.DTOs;
using CarShowRoom.DAL.Models;
using CarShowRoom.DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthRepository _authRepo;
    private readonly IConfiguration _configuration;

    public AuthController(AuthRepository authRepo, IConfiguration configuration)
    {
        _authRepo = authRepo;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var userExists = await _authRepo.GetUserByEmailAsync(registerDto.Email);
        if (userExists != null)
        {
            return BadRequest("This email is already registered.");
        }

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        var newUser = new User
        {
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            Password = hashedPassword,
            Role = registerDto.Role,
            Phone = registerDto.Phone,
            Address = registerDto.Address
        };

        int userId = await _authRepo.RegisterAsync(newUser);

        return Ok(new { Message = "User registered successfully", UserId = userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await _authRepo.GetUserByEmailAsync(loginDto.Email);

        if (user == null)
        {
            return Unauthorized("Invalid Email or Password.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);

        if (!isPasswordValid)
        {
            return Unauthorized("Invalid Email or Password.");
        }

        string token = CreateJwtToken(user);

        return Ok(new { Token = token, Message = "Login Successful" });
    }

    [Authorize]
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized(new { Message = "Unauthorized access. User ID not found in token." });
        }

        int userId = int.Parse(userIdClaim.Value);

        var success = await _authRepo.UpdateProfileAsync(userId, model.FullName, model.Phone, model.Address);

        if (success)
        {
            return Ok(new { Message = "Profile updated successfully." });
        }

        return BadRequest(new { Message = "Failed to update profile. Please try again." });
    }

    private string CreateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);

        var user = await _authRepo.GetUserByIdAsync(userId);
        if (user == null) return NotFound(new { Message = "User not found." });

        if (!BCrypt.Net.BCrypt.Verify(model.OldPassword, user.Password))
        {
            return BadRequest(new { Message = "Current password is incorrect." });
        }

        string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

        var success = await _authRepo.UpdatePasswordAsync(userId, newHashedPassword);

        if (success) return Ok(new { Message = "Password changed successfully." });

        return BadRequest(new { Message = "Failed to change password." });
    }
    [Authorize]
    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim == null) return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);

        var success = await _authRepo.DeleteAccountAsync(userId);

        if (success)
        {
            return Ok(new { Message = "Account deleted successfully. We're sorry to see you go!" });
        }

        return BadRequest(new { Message = "Failed to delete the account. Please contact support." });
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);

        var profile = await _authRepo.GetUserProfileAsync(userId);

        if (profile == null)
            return NotFound(new { Message = "User profile not found." });

        return Ok(profile);
    }

}