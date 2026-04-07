using EDMS.API.Models;
using EDMS.Core.Interfaces;
using EDMS.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EDMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IStaffUserRepository _users;
    private readonly PasswordService _passwordService;
    private readonly JwtService _jwtService;

    public AuthController(IStaffUserRepository users, PasswordService passwordService, JwtService jwtService)
    {
        _users = users;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var user = await _users.GetByUsernameAsync(request.Username);
        if (user is null || !_passwordService.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new ApiResponse<object> { Success = false, Message = "Invalid credentials." });
        }

        await _users.UpdateLastLoginAsync(user.UserId);
        var token = _jwtService.GenerateToken(user);

        return Ok(new ApiResponse<LoginResponse>
        {
            Success = true,
            Data = new LoginResponse
            {
                Token = token,
                Role = user.Role.ToString(),
                UserId = user.UserId
            },
            Message = "Login successful."
        });
    }
}
