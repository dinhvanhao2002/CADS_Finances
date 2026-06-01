using CADSFINANCE.Services.Application.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace CADSFINANCE.Controllers;

[ApiController]
[Route("api/websocket-notifications")]
public sealed class WebSocketNotificationsController : ControllerBase
{
    private readonly IWebSocketConnectionManager _connectionManager;
    private readonly IWebSocketMessagePublisher _publisher;

    public WebSocketNotificationsController(
        IWebSocketConnectionManager connectionManager,
        IWebSocketMessagePublisher publisher)
    {
        _connectionManager = connectionManager;
        _publisher = publisher;
    }

    [HttpGet("clients")]
    public ActionResult<IReadOnlyCollection<string>> GetClients()
    {
        return Ok(_connectionManager.GetClientIds());
    }

    [HttpPost("broadcast")]
    public async Task<IActionResult> Broadcast([FromBody] WebSocketNotificationRequest request, CancellationToken cancellationToken)
    {
        await _publisher.BroadcastAsync(request.Type, request.Data, cancellationToken);
        return Accepted();
    }

    [HttpPost("clients/{clientId}")]
    public async Task<IActionResult> SendToClient(
        string clientId,
        [FromBody] WebSocketNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var sent = await _publisher.SendToClientAsync(clientId, request.Type, request.Data, cancellationToken);
        return sent ? Accepted() : NotFound(new { message = $"WebSocket client '{clientId}' is not connected." });
    }
}

public sealed class WebSocketNotificationRequest
{
    public string Type { get; init; } = "notification";

    public object? Data { get; init; }
}
