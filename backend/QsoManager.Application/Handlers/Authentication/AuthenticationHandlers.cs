using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using QsoManager.Application.Commands.Authentication;
using QsoManager.Application.Commands.ModeratorAggregate;
using QsoManager.Application.DTOs.Authentication;
using QsoManager.Application.Interfaces.Auth;
using QsoManager.Domain.Repositories;
using System.Security.Claims;

namespace QsoManager.Application.Handlers.Authentication;

public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenDto>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IModeratorAggregateRepository _moderatorRepository;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IAuthenticationService authenticationService, 
        IModeratorAggregateRepository moderatorRepository,
        ILogger<LoginCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _moderatorRepository = moderatorRepository;
        _logger = logger;
    }    public async Task<TokenDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await _authenticationService.LoginAsyncByUserName(request.Username, request.Password);

            // Récupérer les informations du modérateur
            var moderatorResult = await _moderatorRepository.GetByIdAsync(Guid.Parse(userId));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, request.Username)
            };

            var token = _authenticationService.CreateToken(claims);
              // Enrichir le TokenDto avec les informations du modérateur si disponibles
            if (moderatorResult.IsSuccess)
            {
                moderatorResult.Match(
                    moderator =>
                    {
                        token.Email = moderator.Email;
                        token.QrzUsername = moderator.QrzUsername;
                        return token;
                    },
                    _ => token
                );
            }

            return token;
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
    private readonly IModeratorAggregateRepository _moderatorRepository;
    private readonly ILogger<LoginByEmailCommandHandler> _logger;

    public LoginByEmailCommandHandler(
        IAuthenticationService authenticationService, 
        IModeratorAggregateRepository moderatorRepository,
        ILogger<LoginByEmailCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _moderatorRepository = moderatorRepository;
        _logger = logger;
    }

    public async Task<TokenDto> Handle(LoginByEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await _authenticationService.LoginAsyncByEmail(request.Email, request.Password);

            // Récupérer les informations du modérateur
            var moderatorResult = await _moderatorRepository.GetByIdAsync(Guid.Parse(userId));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Email, request.Email)
            };

            var token = _authenticationService.CreateToken(claims);
            
            // Enrichir le TokenDto avec les informations du modérateur si disponibles
            if (moderatorResult.IsSuccess)
            {
                moderatorResult.Match(
                    moderator =>
                    {
                        token.Email = moderator.Email;
                        token.QrzUsername = moderator.QrzUsername;
                        return token;
                    },
                    _ => token
                );
            }

            return token;
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
    private readonly IMediator _mediator;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IAuthenticationService authenticationService, 
        IMediator mediator,
        ILogger<RegisterCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _mediator = mediator;
        _logger = logger;
    }    public async Task<string> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Créer l'utilisateur
            var userId = await _authenticationService.Register(request.UserName, request.Password, request.Email);
            
            // Créer automatiquement un modérateur associé avec le même ID utilisateur
            // et utiliser le username comme CallSign
            var createModeratorCommand = new CreateModeratorCommand(
                Guid.Parse(userId),
                request.UserName,
                request.Email
            );
            
            var moderatorResult = await _mediator.Send(createModeratorCommand, cancellationToken);
            
            // Vérifier si la création du modérateur a réussi
            moderatorResult.Match(
                success => _logger.LogInformation("Utilisateur {UserName} créé avec succès avec modérateur associé ID {ModeratorId}", 
                    request.UserName, success.Id),
                errors => _logger.LogWarning("Utilisateur {UserName} créé mais erreur lors de la création du modérateur: {Errors}", 
                    request.UserName, string.Join(", ", errors.Select(e => e.Message)))
            );
            
            return userId;
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
