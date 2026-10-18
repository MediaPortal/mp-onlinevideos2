using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace WebServiceCore.Services
{

    public class UserService(Credentials credentials) : IUserService
    {
        public async Task<ClaimsPrincipal> TryAuthenticate(string username, string password)
        {
            var user = await credentials.Authenticate(username, password);
            if (user == null)
                return null;

            var claims = new List<Claim> {
                new(ClaimTypes.Name, user.Email),
                new(ClaimTypes.Role, "user"),
                new("credential-stamp", Credentials.Stamp(user)) };

            if (user.IsAdmin)
                claims.Add(new(ClaimTypes.Role, "admin"));

            return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        }
    }
}
