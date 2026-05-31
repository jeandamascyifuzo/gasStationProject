using Serilog.Context;
using System.Security.Claims;

namespace Escale.API.Middleware;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User?.FindFirst("UserId")?.Value;
        var userName = context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var role = context.User?.FindFirst(ClaimTypes.Role)?.Value;

        using (LogContext.PushProperty("UserId", userId ?? "anonymous"))
        using (LogContext.PushProperty("UserName", userName ?? "anonymous"))
        using (LogContext.PushProperty("UserRole", role ?? ""))
        {
            await _next(context);
        }
    }
}
