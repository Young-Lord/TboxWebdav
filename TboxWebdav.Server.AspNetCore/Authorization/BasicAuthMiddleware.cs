using System.Text;
using TboxWebdav.Server.Modules.Tbox;

public class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string AuthenticationHeaderName = "Authorization";

    public BasicAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
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

        if (IsValidUser(username, password))
        {
            //_userTokenProvider.SetUserToken(password);
            context.Items["UserToken"] = password;
            await _next(context);
        }
        else
        {
            await SendUnauthorizedResponse(context);
        }
    }

    private bool IsValidUser(string username, string password)
    {
        return true;
    }

    private async Task SendUnauthorizedResponse(HttpContext context)
    {
        context.Response.StatusCode = 401; // Unauthorized
        context.Response.Headers["WWW-Authenticate"] = "Basic";
        await context.Response.WriteAsync("Unauthorized");
    }
}