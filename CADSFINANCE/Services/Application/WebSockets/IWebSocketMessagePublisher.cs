namespace CADSFINANCE.Services.Application.WebSockets;

public interface IWebSocketMessagePublisher
{
    Task<bool> SendToClientAsync(string clientId, string type, object? data, CancellationToken cancellationToken = default);

    Task BroadcastAsync(string type, object? data, CancellationToken cancellationToken = default);
}
