using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QsoManager.Application.Commands.Authentication;
using QsoManager.Application.Commands.ModeratorAggregate;
using QsoManager.Application.DTOs;
using QsoManager.Application.DTOs.Authentication;

namespace QsoManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var command = new LoginCommand(request.UserName, request.Password);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt for user {UserName}", request.UserName);
            return Unauthorized(new { message = "Invalid credentials" });
        }
    }

    [HttpPost("login-email")]
    public async Task<ActionResult<TokenDto>> LoginByEmail([FromBody] LoginByEmailRequestDto request)
    {
        try
        {
            var command = new LoginByEmailCommand(request.Email, request.Password);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt for email {Email}", request.Email);
            return Unauthorized(new { message = "Invalid credentials" });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<string>> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var command = new RegisterCommand(request.UserName, request.Password, request.Email);
            var result = await _mediator.Send(command);
            return Ok(new { userId = result, message = "User created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {UserName}", request.UserName);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        try
        {
            var command = new ForgotPasswordCommand(request.Email, request.ResetPasswordUrl);
            var result = await _mediator.Send(command);
            
            if (result)
                return Ok(new { message = "Password reset email sent successfully" });
            else
                return BadRequest(new { message = "Failed to send password reset email" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for email {Email}", request.Email);
            return BadRequest(new { message = "An error occurred while processing your request" });
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            var command = new ResetPasswordCommand(request.UserId, request.ResetToken, request.Password);
            var result = await _mediator.Send(command);
            
            if (result)
                return Ok(new { message = "Password reset successfully" });
            else
                return BadRequest(new { message = "Failed to reset password" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for user {UserId}", request.UserId);
            return BadRequest(new { message = "An error occurred while resetting your password" });
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<ModeratorDto>> UpdateProfile([FromBody] UpdateProfileRequestDto request)
    {
        try
        {
            var command = new UpdateModeratorCommand(
                Email: request.Email,
                QrzUsername: request.QrzUsername,
                QrzPassword: request.QrzPassword,
                User: User
            );
            
            var result = await _mediator.Send(command);
            
            if (result.IsSuccess)
            {
                return Ok(result.Match(dto => dto, _ => throw new InvalidOperationException()));
            }
            else
            {
                var errors = result.Match(_ => Array.Empty<string>(), errors => errors.Select(e => e.Message).ToArray());
                return BadRequest(new { errors });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during profile update for user {UserId}", User?.Identity?.Name);
            return BadRequest(new { message = "An error occurred while updating your profile" });
        }
    }
}
