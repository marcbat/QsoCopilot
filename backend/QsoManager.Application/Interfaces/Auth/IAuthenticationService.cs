using QsoManager.Application.DTOs.Authentication;
using System.Security.Claims;

namespace QsoManager.Application.Interfaces.Auth;

public interface IAuthenticationService
{
    Task<string> LoginAsyncByUserName(string userName, string password);
    Task<string> LoginAsyncByEmail(string email, string password);
    Task<string> Register(string userName, string password, string email);
    Task<bool> UserListEmpty();
    Task<string> GeneratePasswordResetUrlCallBackAsync(string resetPasswordUrl, string userEmail);
    Task ResetPassword(string userId, string resetToken, string password);
    Task SignInAsync(string id, IEnumerable<Claim>? additionalClaims = null);
    TokenDto CreateToken(IEnumerable<Claim> additionalClaims);
}
