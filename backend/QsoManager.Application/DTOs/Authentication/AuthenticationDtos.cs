namespace QsoManager.Application.DTOs.Authentication;

public class RegisterRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginByEmailRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string ResetPasswordUrl { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
