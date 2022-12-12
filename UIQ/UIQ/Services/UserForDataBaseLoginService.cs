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

        public async Task<bool> CheckPasswordCorrect(string password)
        {
            var userId = _httpContext.User.Claims.FirstOrDefault(x => x.Type == "Id").Value ?? string.Empty;
            var sql = @"SELECT COUNT(1) FROM `user`
                        WHERE `user_id` = @UserId
                        AND `passwd` = @Password";

            var result = await _dataBaseNcsUiService.QueryAsync<int>(sql, new { UserId = userId, Password = password.ToMD5() });
            return result.FirstOrDefault() > 0;
        }

        public async Task<bool> ChangePassword(string newPassword)
        {
            var userId = _httpContext.User.Claims.FirstOrDefault(x => x.Type == "Id").Value ?? string.Empty;
            var result = await _dataBaseNcsUiService.UpdateAsync("user", new { passwd = newPassword.ToMD5() }, new { user_id = userId });
            return result > 0;
        }

        #region Private Methods

        private async Task<UserViewModel> GetUserInfo(string userId, string password)
        {
            var sql = $@"SELECT u.*, g.`group_name`, (SELECT GROUP_CONCAT(`role_id`)
                                                      FROM `role_user` ru
                                                      WHERE ru.`user_id` = u.`user_id`) AS `roles`
                         FROM `user` u
                         JOIN `group` g ON g.`group_id` = u.`group_id`
                         WHERE `account` = @Account AND `passwd` = @Password";

            return _dataBaseNcsUiService.QueryAsync<UserViewModel>(sql, new { Account = userId, Password = password }).GetAwaiter().GetResult().FirstOrDefault();
        }

        private async Task SetLogin(UserViewModel userInfo)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userInfo.Account),
                new Claim("Id", userInfo.User_Id.ToString()),
                new Claim(ClaimTypes.Role, userInfo.Group_Name),
                new Claim("GourpId", userInfo.Group_Id.ToString()),
                new Claim("RoleIds", (userInfo.Roles ?? string.Empty)),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await _httpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity));
        }

        #endregion Private Methods
    }
}