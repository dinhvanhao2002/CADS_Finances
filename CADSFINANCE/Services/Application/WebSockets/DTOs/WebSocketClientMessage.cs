using System.Text.Json;

namespace CADSFINANCE.Services.Application.WebSockets.DTOs;

public sealed class WebSocketClientMessage
{
    public string Type { get; init; } = string.Empty;

    public JsonElement? Data { get; init; }

    public string? TraceId { get; init; }
}
