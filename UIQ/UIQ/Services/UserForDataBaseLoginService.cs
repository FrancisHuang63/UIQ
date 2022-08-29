using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using UIQ.Services.Interfaces;
using UIQ.ViewModels;

namespace UIQ.Services
{
    public class UserForDataBaseLoginService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataBaseService _dataBaseService;
        private HttpContext _httpContext;

        public UserForDataBaseLoginService(IHttpContextAccessor httpContextAccessor, IDataBaseService dataBaseService)
        {
            _httpContextAccessor = httpContextAccessor;
            _dataBaseService = dataBaseService;
            _httpContext = _httpContextAccessor.HttpContext;
        }

        public async Task<bool> Login(string account, string password)
        {
            var userInfo = await GetUserInfo(account, password.ToMD5());
            if (userInfo == null) return false;

            await SetLogin(userInfo);
            return true;
        }
        
        public async Task Logout()
        {
            await _httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        #region Private Methods

        private async Task<UserViewModel> GetUserInfo(string userId, string password)
        {
            var sql = $@"SELECT `user`.*, `group`.`group_name` FROM `user`
                         JOIN `group` ON `user`.`group_id` = `group`.`group_id`
                         WHERE `account` = @Account AND `passwd` = @Password";

            return _dataBaseService.QueryAsync<UserViewModel>(sql, new { Account = userId, Password = password }).GetAwaiter().GetResult().Single();
        }

        private async Task SetLogin(UserViewModel userInfo)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userInfo.Account),
                new Claim("Id", userInfo.UserId.ToString()),
                new Claim(ClaimTypes.Role, userInfo.GroupId.ToString()),
                new Claim("GourpName", userInfo.GroupName),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await _httpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));

            //Set session
            _httpContext.Session.SetString("login", userInfo.Account);
            _httpContext.Session.SetString("group", userInfo.GroupName);
        }

        #endregion Private Methods
    }
}