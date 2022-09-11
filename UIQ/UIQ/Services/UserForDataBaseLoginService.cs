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
        private readonly IDataBaseService _dataBaseNcsUiService;
        private HttpContext _httpContext;

        public UserForDataBaseLoginService(IHttpContextAccessor httpContextAccessor, IEnumerable<IDataBaseService> dataBaseServices)
        {
            _httpContextAccessor = httpContextAccessor;
            _dataBaseNcsUiService = dataBaseServices.Single(x => x.DataBase == Enums.DataBaseEnum.NcsUi);
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

            return _dataBaseNcsUiService.QueryAsync<UserViewModel>(sql, new { Account = userId, Password = password }).GetAwaiter().GetResult().Single();
        }

        private async Task SetLogin(UserViewModel userInfo)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userInfo.Account),
                new Claim("Id", userInfo.UserId.ToString()),
                new Claim(ClaimTypes.Role, userInfo.GroupName),
                new Claim("GourpId", userInfo.GroupId.ToString()),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await _httpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));
        }

        #endregion Private Methods
    }
}