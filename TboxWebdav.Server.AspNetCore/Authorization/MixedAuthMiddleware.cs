using System.Text;
using TboxWebdav.Server.AspNetCore.Models;

public class MixedAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string AuthenticationHeaderName = "Authorization";

    public MixedAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (AppCmdOption.Default.AuthMode == AppAuthMode.None)
        {
            var user = AppCmdOption.Default.Users.FirstOrDefault(u => u.UserName == null && u.Password == null);
            if (user != null)
            {
                if (user.UserToken != null)
                {
                    context.Items["UserToken"] = user.UserToken;
                    await _next(context);
                    return;
                }
                else if (user.Cookie != null)
                {
                    context.Items["JaCookie"] = user.Cookie;
                    await _next(context);
                    return;
                }
            }
            await SendUnauthorizedResponse(context);
            return;
        }
        if (!context.Request.Headers.TryGetValue(AuthenticationHeaderName, out var authHeaders))
        {
            await SendUnauthorizedResponse(context);
            return;
        }

        var authHeader = authHeaders.FirstOrDefault();

        if (authHeader == null || authHeader.ToString().Length == 0)
        {
            await SendUnauthorizedResponse(context);
            return;
        }

        if (!authHeader.ToString().StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            await SendUnauthorizedResponse(context);
            return;
        }

        var encodedUsernamePassword = authHeader.ToString().Substring("Basic ".Length).Trim();
        var encoding = Encoding.GetEncoding("iso-8859-1");
        var usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

        var username = usernamePassword.Split(':')[0];
        var password = usernamePassword.Split(':')[1];

        if (IsValidUserToken(password) && (AppCmdOption.Default.AuthMode == AppAuthMode.UserToken || AppCmdOption.Default.AuthMode == AppAuthMode.Mixed))
        {
            context.Items["UserToken"] = password;
            await _next(context);
            return;
        }
        if (IsValidJaCookie(password) && (AppCmdOption.Default.AuthMode == AppAuthMode.JaCookie || AppCmdOption.Default.AuthMode == AppAuthMode.Mixed))
        {
            context.Items["JaCookie"] = password;
            await _next(context);
            return;
        }
        if (AppCmdOption.Default.AuthMode == AppAuthMode.Custom || AppCmdOption.Default.AuthMode == AppAuthMode.Mixed)
        {
            var user = AppCmdOption.Default.Users.FirstOrDefault(u => u.UserName != null && u.UserName == username && u.Password == password);
            if (user != null)
            {
                if (user.UserToken != null)
                {
                    context.Items["UserToken"] = user.UserToken;
                    await _next(context);
                    return;
                }
                else if (user.Cookie != null)
                {
                    context.Items["JaCookie"] = user.Cookie;
                    await _next(context);
                    return;
                }
            }
        }
        await SendUnauthorizedResponse(context);
    }

    private bool IsValidUserToken(string password)
    {
        return password.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')) && password.Length == 128;
    }
    
    private bool IsValidJaCookie(string password)
    {
        try
        {
            var test = Convert.FromBase64String(password);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private async Task SendUnauthorizedResponse(HttpContext context)
    {
        context.Response.StatusCode = 401; // Unauthorized
        context.Response.Headers["WWW-Authenticate"] = "Basic";
        await context.Response.WriteAsync("Unauthorized");
    }
}