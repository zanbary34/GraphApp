using Microsoft.AspNetCore.Mvc;
using ServiceB.Services.WebSockets;
using System.Reflection.Metadata;

public class IncomeController : ControllerBase
{
    private readonly WebSocketIncomeHandler _handler;

    public IncomeController(WebSocketIncomeHandler handler)
    {
        _handler = handler;
    }

    [Route("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _handler.HandleWebSocketConnection(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
