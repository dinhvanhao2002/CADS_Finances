using System.Net.WebSockets;

namespace CADSFINANCE.Services.Application.WebSockets;

public interface IWebSocketConnectionManager
{
    IReadOnlyCollection<string> GetClientIds();

    Task HandleConnectionAsync(string clientId, WebSocket socket, CancellationToken cancellationToken);

    Task<bool> SendAsync(string clientId, object payload, CancellationToken cancellationToken = default);

    Task BroadcastAsync(object payload, CancellationToken cancellationToken = default);
}
