using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseUrls("http://[::]:21001").UseKestrel(options => { options.Limits.MaxRequestBodySize = long.MaxValue; });

var app = builder.Build();

app.UseWebSockets();

var clients = new List<WebSocket>();

app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        lock (clients)
        {
            clients.Add(webSocket);
        }
        await HandleWebSocketAsync(webSocket, clients);
    }
    else
    {
        await next();
    }
});

async Task HandleWebSocketAsync(WebSocket webSocket, List<WebSocket> clients)
{
    var buffer = new byte[1024 * 4];

    try
    {
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                lock (clients)
                {
                    clients.Remove(webSocket);
                }
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            else if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // Broadcast message to all connected clients
                lock (clients)
                {
                    foreach (var client in clients)
                    {
                        if (client.State == WebSocketState.Open)
                        {
                            var data = Encoding.UTF8.GetBytes(message);
                            client.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }
                }
            }
            else if (result.MessageType == WebSocketMessageType.Binary)
            {
                // Process the binary data (e.g., save it to a file)
                var fileName = $"received_{DateTime.UtcNow.Ticks}.bin";
                await File.WriteAllBytesAsync(fileName, buffer.Take(result.Count).ToArray());

                Console.WriteLine($"Received binary data ({result.Count} bytes). Saved as {fileName}.");

                // Send acknowledgment as binary response
                var acknowledgment = Encoding.UTF8.GetBytes($"Binary data received: {result.Count} bytes.");
                await webSocket.SendAsync(new ArraySegment<byte>(acknowledgment), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        lock (clients)
        {
            clients.Remove(webSocket);
        }
    }
}

app.Run();