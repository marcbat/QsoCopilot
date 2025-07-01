using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QsoManager.Application.DTOs.Authentication;
using QsoManager.Application.Exceptions;
using QsoManager.Application.Interfaces.Auth;
using QsoManager.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace QsoManager.Infrastructure.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager, 
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    public async Task<string> LoginAsyncByUserName(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user != null && await _userManager.CheckPasswordAsync(user, password))
        {
            return user.Id;
        }
        else
        {
            throw new AuthenticationException($"Impossible de connecter l'utilisateur {userName}.");
        }
    }

    public async Task<string> LoginAsyncByEmail(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user != null && await _userManager.CheckPasswordAsync(user, password))
        {
            return user.Id;
        }
        else
        {
            throw new AuthenticationException($"Impossible de connecter l'utilisateur avec l'email {email}.");
        }
    }

    public TokenDto CreateToken(IEnumerable<Claim> additionalClaims)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "JWTAuthenticationHIGHsecuredPasswordVVVp1OH7Xzyr";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "http://localhost:5000";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "http://localhost:4200";
        var jwtExpiryHours = int.Parse(_configuration["Jwt:ExpiryHours"] ?? "3");

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            expires: DateTime.Now.AddHours(jwtExpiryHours),
            claims: additionalClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );        var tokenDto = new TokenDto 
        { 
            Token = new JwtSecurityTokenHandler().WriteToken(token), 
            Expiration = token.ValidTo,
            UserId = additionalClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "",
            UserName = additionalClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? ""
        };

        return tokenDto;
    }

    public async Task<string> Register(string userName, string password, string email)
    {
        var userExistsByName = await _userManager.FindByNameAsync(userName);
        if (userExistsByName != null)
            throw new UserAlreadyExistsException($"L'utilisateur {userName} existe déjà.");
        
        var userExistsByEmail = await _userManager.FindByEmailAsync(email);
        if (userExistsByEmail != null)
            throw new UserAlreadyExistsException($"Il existe déjà un utilisateur avec l'adresse {email}.");

        ApplicationUser user = new()
        {
            Email = email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = userName
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new UserRegistrationFailedException(string.Join(',', result.Errors.Select(e => e.Description)));

        return user.Id;
    }    public async Task SignInAsync(string id, IEnumerable<Claim>? additionalClaims = null)
    {
        var user = await _userManager.FindByIdAsync(id) ?? 
            throw new AuthenticationException($"Impossible de trouver l'utilisateur {id}.");

        // Pour cette implémentation, nous utilisons seulement SignInAsync
        // Les claims supplémentaires peuvent être gérés via le JWT token
        await _signInManager.SignInAsync(user, false);
    }

    public Task<bool> UserListEmpty()
    {
        return Task.FromResult(!_userManager.Users.Any());
    }    public async Task<string> GeneratePasswordResetUrlCallBackAsync(string resetPasswordUrl, string userEmail)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is null)
            throw new UserNotFoundException($"Erreur de génération du reset password, utilisateur non trouvé avec l'email {userEmail}");

        // Validate the reset password URL
        if (!Uri.TryCreate(resetPasswordUrl, UriKind.Absolute, out var validatedUri))
            throw new ArgumentException($"Invalid reset password URL: {resetPasswordUrl}");

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var uriBuilder = new UriBuilder(resetPasswordUrl);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["userId"] = user.Id;
        query["code"] = token.ToString();

        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString();
    }

    public async Task ResetPassword(string userId, string resetToken, string password)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new UserNotFoundException($"Erreur de reset password, utilisateur non trouvé avec l'ID {userId}");

        var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetToken));
        var result = await _userManager.ResetPasswordAsync(user, token, password);
        if (!result.Succeeded)
            throw new PasswordResetFailedException(string.Join(',', result.Errors.Select(e => e.Description)));
    }
}
