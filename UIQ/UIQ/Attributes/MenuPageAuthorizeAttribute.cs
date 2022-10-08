using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UIQ.Enums;
using UIQ.Services.Interfaces;

namespace UIQ.Attributes
{
    public class MenuPageAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public MenuEnum Menu { get; set; }

        public MenuPageAuthorizeAttribute(MenuEnum menu)
        {
            Menu = menu;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new ForbidResult();
                return;
            }

            var uiqService = context.HttpContext.RequestServices.GetService(typeof(IUiqService)) as IUiqService;
            var menus = uiqService.GetMenuItemsWithPermissonAsync().GetAwaiter().GetResult();
            if (menus.Any(x => x.Menu_Id == (int)Menu) == false)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}