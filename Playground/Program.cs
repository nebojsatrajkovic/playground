using Core.Shared;
using Core.Shared.ExceptionHandling;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseUrls("http://[::]:21000").UseKestrel(options => { options.Limits.MaxRequestBodySize = long.MaxValue; });

builder.Services.AddControllers().AddJsonOptions(json =>
{
    json.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddCors()
    .AddHttpContextAccessor()
    .AddLocalization(opts => { opts.ResourcesPath = "Resources"; })
    .Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.All;
    });

// NOTE: include controllers from core library
builder.Services.AddMvc().AddApplicationPart(typeof(Core.Shared.Controllers.LongPollingController).Assembly);

builder.Logging.AddLog4Net();

builder.Initialize_CORE_Configuration();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger()
        .UseSwaggerUI();
}

app.MapControllers();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.UseForwardedHeaders()
    .ConfigureCoreExceptionHandler();

// InitializeWS(); - example how to initiate web socket connection

app.Run();


async void InitializeWS()
{
    var uri = new Uri("ws://localhost:21001");
    using (var client = new ClientWebSocket())
    {
        Console.WriteLine("Connecting to WebSocket server...");
        await client.ConnectAsync(uri, CancellationToken.None);
        Console.WriteLine("Connected!");

        var receiveTask = ReceiveMessagesAsync(client);

        // Send messages to the server
        while (client.State == WebSocketState.Open)
        {
            var message = DateTime.Now.ToString();

            var data = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);

            await Task.Delay(3000);
        }

        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
        Console.WriteLine("Connection closed.");
    }
}

async Task ReceiveMessagesAsync(ClientWebSocket client)
{
    var buffer = new byte[1024 * 4];
    try
    {
        while (client.State == WebSocketState.Open)
        {
            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"\nReceived: {message}");
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("Server closed the connection.");
                break;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error receiving messages: {ex.Message}");
    }
}