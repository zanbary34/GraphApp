using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ServiceB.Services.WebSockets;


public class WebSocketMiddleware(IConfiguration configuration, RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
{
    private readonly RequestDelegate _next = next;
    private readonly string _secretKey = configuration["JwtSettings:SecretKey"] ?? throw new ArgumentNullException("JwtSettings:SecretKey");
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var token = context.Request.Query["token"].ToString();
            if (string.IsNullOrEmpty(token) || !ValidateJwtToken(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized WebSocket connection");
                return;
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var webSocketHandler = scope.ServiceProvider.GetRequiredService<WebSocketIncomeHandler>();

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await webSocketHandler.HandleWebSocketConnection(webSocket);
        }
        else
        {
            await _next(context);
        }
    }

    private bool ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = "serviceA",     
                ValidAudience = "serviceB",   
                ClockSkew = TimeSpan.Zero     
            }, out SecurityToken validatedToken);

            return validatedToken != null;
        }
        catch
        {
            return false;
        }
    }
}
