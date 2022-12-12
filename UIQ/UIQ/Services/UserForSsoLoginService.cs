using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using UIQ.Services.Interfaces;

namespace UIQ.Services
{
    public class UserForSsoLoginService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpContext _httpContext;

        public UserForSsoLoginService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpContext = _httpContextAccessor.HttpContext;
        }

        public Task<bool> ChangePassword(string newPassword)
        {
            return Task.FromResult(false);
        }

        public Task<bool> CheckPasswordCorrect(string password)
        {
            return Task.FromResult(false);
        }

        public async Task<bool> Login(string userId, string password)
        {
            throw new NotImplementedException();
        }

        public async Task Logout()
        {
            await _httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }
    }
}