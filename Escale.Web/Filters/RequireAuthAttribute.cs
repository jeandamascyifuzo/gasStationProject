using Escale.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Escale.Web.Filters;

public class RequireAuthAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var controllerName = context.RouteData.Values["controller"]?.ToString();
        if (string.Equals(controllerName, "Account", StringComparison.OrdinalIgnoreCase))
        {
            base.OnActionExecuting(context);
            return;
        }

        var token = context.HttpContext.Request.Cookies[TokenHelper.AccessTokenCookie];
        if (string.IsNullOrEmpty(token))
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        base.OnActionExecuting(context);
    }

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        var controllerName = context.RouteData.Values["controller"]?.ToString();
        if (string.Equals(controllerName, "Account", StringComparison.OrdinalIgnoreCase))
        {
            base.OnResultExecuting(context);
            return;
        }

        // If cookies were cleared during the request (expired token + failed refresh), redirect to login
        var token = context.HttpContext.Request.Cookies[TokenHelper.AccessTokenCookie];
        var tokenDeleted = context.HttpContext.Response.Headers["Set-Cookie"]
            .Any(c => c != null && c.Contains(TokenHelper.AccessTokenCookie) && c.Contains("expires=Thu, 01 Jan 1970"));

        if (tokenDeleted || string.IsNullOrEmpty(token))
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        base.OnResultExecuting(context);
    }
}
