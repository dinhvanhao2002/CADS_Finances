using CADSFINANCE.Services.Application.WebSockets.DTOs;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace CADSFINANCE.Services.Application.WebSockets;

public sealed class WebSocketConnectionManager : IWebSocketConnectionManager
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private readonly ILogger<WebSocketConnectionManager> _logger;

    public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
    {
        _logger = logger;
    }

    public IReadOnlyCollection<string> GetClientIds()
    {
        return _connections.Keys.OrderBy(clientId => clientId).ToArray();
    }

    public async Task HandleConnectionAsync(string clientId, WebSocket socket, CancellationToken cancellationToken)
    {
        await ReplaceConnectionAsync(clientId, socket, cancellationToken);
        await SendAsync(clientId, new WebSocketEnvelope("connected", new { clientId }), cancellationToken);

        try
        {
            await ReceiveLoopAsync(clientId, socket, cancellationToken);
        }
        finally
        {
            if (_connections.TryRemove(new KeyValuePair<string, WebSocket>(clientId, socket)))
            {
                _logger.LogInformation("WebSocket client disconnected: {ClientId}", clientId);
            }

            socket.Dispose();
        }
    }

    public async Task<bool> SendAsync(string clientId, object payload, CancellationToken cancellationToken = default)
    {
        if (!_connections.TryGetValue(clientId, out var socket) || socket.State != WebSocketState.Open)
        {
            return false;
        }

        return await SendSocketAsync(socket, payload, cancellationToken);
    }

    public async Task BroadcastAsync(object payload, CancellationToken cancellationToken = default)
    {
        foreach (var clientId in GetClientIds())
        {
            var sent = await SendAsync(clientId, payload, cancellationToken);
            if (!sent)
            {
                _connections.TryRemove(clientId, out _);
            }
        }
    }

    private async Task ReplaceConnectionAsync(string clientId, WebSocket socket, CancellationToken cancellationToken)
    {
        if (_connections.TryRemove(clientId, out var existing))
        {
            await CloseSocketAsync(existing, "Connection replaced", cancellationToken);
        }

        _connections[clientId] = socket;
        _logger.LogInformation("WebSocket client connected: {ClientId}", clientId);
    }

    private async Task ReceiveLoopAsync(string clientId, WebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];

        while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
        {
            using var message = new MemoryStream();
            WebSocketReceiveResult result;

            do
            {
                result = await socket.ReceiveAsync(buffer, cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await CloseSocketAsync(socket, "Client closed connection", cancellationToken);
                    return;
                }

                if (result.MessageType != WebSocketMessageType.Text)
                {
                    await CloseSocketAsync(socket, "Only text messages are supported", cancellationToken);
                    return;
                }

                message.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            await HandleClientMessageAsync(clientId, message.ToArray(), cancellationToken);
        }
    }

    private async Task HandleClientMessageAsync(string clientId, byte[] messageBytes, CancellationToken cancellationToken)
    {
        var rawMessage = Encoding.UTF8.GetString(messageBytes);
        WebSocketClientMessage? clientMessage = null;

        try
        {
            clientMessage = JsonSerializer.Deserialize<WebSocketClientMessage>(rawMessage, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid WebSocket message from {ClientId}: {Message}", clientId, rawMessage);
        }

        if (clientMessage is not null &&
            string.Equals(clientMessage.Type, "ping", StringComparison.OrdinalIgnoreCase))
        {
            await SendAsync(clientId, new WebSocketEnvelope("pong", new { clientId }, clientMessage.TraceId), cancellationToken);
            return;
        }

        await SendAsync(
            clientId,
            new WebSocketEnvelope("ack", new { clientId, receivedType = clientMessage?.Type ?? "unknown" }, clientMessage?.TraceId),
            cancellationToken);
    }

    private static async Task<bool> SendSocketAsync(WebSocket socket, object payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        try
        {
            await socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
            return true;
        }
        catch (WebSocketException)
        {
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private static async Task CloseSocketAsync(WebSocket socket, string reason, CancellationToken cancellationToken)
    {
        if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, cancellationToken);
        }

        socket.Dispose();
    }
}
