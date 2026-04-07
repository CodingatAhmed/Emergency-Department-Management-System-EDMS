namespace EDMS.API.Models;

public record LoginRequest(string Username, string Password);

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}
