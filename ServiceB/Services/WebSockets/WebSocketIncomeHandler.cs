using ServiceB.Data;
using ServiceB.Data.Models;
using System.Net.WebSockets;
using System.Text;

namespace ServiceB.Services.WebSockets
{
    public class WebSocketIncomeHandler(IGraphService context)
    {
        private readonly IGraphService _context = context;

        public async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            while (webSocket.State == WebSocketState.Open)
            {
                var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    var response = await _context.UpdateFromMessageAsync(Encoding.UTF8.GetString(buffer, 0, receiveResult.Count));
                    if (response != null)
                    {
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                else if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
                }
            }
        }
    }

}
