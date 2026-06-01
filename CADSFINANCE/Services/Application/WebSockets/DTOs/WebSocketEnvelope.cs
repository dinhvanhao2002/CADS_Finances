using System.Text.Json.Serialization;

namespace CADSFINANCE.Services.Application.WebSockets.DTOs;

public sealed class WebSocketEnvelope
{
    public WebSocketEnvelope(string type, object? data = null, string? traceId = null)
    {
        Type = type;
        Data = data;
        TraceId = traceId;
    }

    public string Type { get; }

    public object? Data { get; }

    public string? TraceId { get; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
}
