using MediatR;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands.Authentication;
using QsoManager.Application.DTOs.Authentication;
using QsoManager.Application.Interfaces.Auth;
using System.Security.Claims;

namespace QsoManager.Application.Handlers.Authentication;

public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenDto>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(IAuthenticationService authenticationService, ILogger<LoginCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<TokenDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await _authenticationService.LoginAsyncByUserName(request.Username, request.Password);

            // TODO: Récupérer les informations utilisateur et créer les claims appropriés
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, request.Username)
            };

            return _authenticationService.CreateToken(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", request.Username);
            throw;
        }
    }
}

public class LoginByEmailCommandHandler : IRequestHandler<LoginByEmailCommand, TokenDto>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<LoginByEmailCommandHandler> _logger;

    public LoginByEmailCommandHandler(IAuthenticationService authenticationService, ILogger<LoginByEmailCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<TokenDto> Handle(LoginByEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await _authenticationService.LoginAsyncByEmail(request.Email, request.Password);

            // TODO: Récupérer les informations utilisateur et créer les claims appropriés
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Email, request.Email)
            };

            return _authenticationService.CreateToken(claims);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email {Email}", request.Email);
            throw;
        }
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, string>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(IAuthenticationService authenticationService, ILogger<RegisterCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<string> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _authenticationService.Register(request.UserName, request.Password, request.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Username}", request.UserName);
            throw;
        }
    }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(IAuthenticationService authenticationService, ILogger<ForgotPasswordCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _authenticationService.GeneratePasswordResetUrlCallBackAsync(request.ResetPasswordUrl, request.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for email {Email}", request.Email);
            return false;
        }
    }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(IAuthenticationService authenticationService, ILogger<ResetPasswordCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _authenticationService.ResetPassword(request.UserId, request.ResetToken, request.Password);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for user {UserId}", request.UserId);
            return false;
        }
    }
}
