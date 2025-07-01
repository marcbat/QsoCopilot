using MediatR;
using QsoManager.Application.DTOs.Authentication;

namespace QsoManager.Application.Commands.Authentication;

public record LoginCommand(string Username, string Password) : IRequest<TokenDto>;
public record LoginByEmailCommand(string Email, string Password) : IRequest<TokenDto>;
public record RegisterCommand(string UserName, string Password, string Email) : IRequest<string>;
public record ForgotPasswordCommand(string Email, string ResetPasswordUrl) : IRequest<bool>;
public record ResetPasswordCommand(string UserId, string ResetToken, string Password) : IRequest<bool>;
