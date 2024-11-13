using ServiceA.Data.Models;
using ServiceA.Services;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class WebSocketSenderHandler
{
    private readonly Uri _serverUri;
    private readonly GenerateJWT _jwtGenerator;
    private ClientWebSocket _webSocket;

    public WebSocketSenderHandler(GenerateJWT jwtGenerator, IConfiguration configuration)
    {
        _jwtGenerator = jwtGenerator;
        var token = _jwtGenerator.GenerateJwtToken(configuration);
        var uri = configuration["WebSocketSettings:Uri"];
        _serverUri = new Uri($"{uri}/ws?token={token}");
        _webSocket = new ClientWebSocket();
    }

    public async Task ConnectAsync()
    {
        if (_webSocket.State != WebSocketState.Open)
        {
            await _webSocket.ConnectAsync(_serverUri, CancellationToken.None);
        }
    }

    public async Task<string?> SendMessageAsync(GraphRequest message)
    {
        await ConnectAsync();

        string messageJson = JsonSerializer.Serialize(message);
        var messageBytes = Encoding.UTF8.GetBytes(messageJson);
        await _webSocket.SendAsync(messageBytes, WebSocketMessageType.Text, true, CancellationToken.None);

        var buffer = new byte[4096];
        var result = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);

        if (result.MessageType == WebSocketMessageType.Text)
        {
            var responseString = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"Received details: {responseString}");
            return responseString;
        }

        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
        return null;
    }
}

