namespace QsoManager.Application.DTOs.Authentication;

public class TokenDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? QrzUsername { get; set; }
}
