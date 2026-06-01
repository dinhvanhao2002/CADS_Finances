using CADSFINANCE.Services.Application.WebSockets.DTOs;

namespace CADSFINANCE.Services.Application.WebSockets;

public sealed class WebSocketMessagePublisher : IWebSocketMessagePublisher
{
    private readonly IWebSocketConnectionManager _connectionManager;

    public WebSocketMessagePublisher(IWebSocketConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public Task<bool> SendToClientAsync(string clientId, string type, object? data, CancellationToken cancellationToken = default)
    {
        return _connectionManager.SendAsync(clientId, new WebSocketEnvelope(type, data), cancellationToken);
    }

    public Task BroadcastAsync(string type, object? data, CancellationToken cancellationToken = default)
    {
        return _connectionManager.BroadcastAsync(new WebSocketEnvelope(type, data), cancellationToken);
    }
}
