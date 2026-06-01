using CADSFINANCE.Services.Application.WebSockets;

namespace CADSFINANCE.Extensions;

public static class WebSocketEndpointExtensions
{
    public static IEndpointRouteBuilder MapCadsWebSockets(this IEndpointRouteBuilder endpoints)
    {
        endpoints.Map("/ws/notifications", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Expected a WebSocket request.");
                return;
            }

            var clientId = context.Request.Query["clientId"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(clientId))
            {
                clientId = context.Connection.Id;
            }

            var connectionManager = context.RequestServices.GetRequiredService<IWebSocketConnectionManager>();
            using var socket = await context.WebSockets.AcceptWebSocketAsync();

            await connectionManager.HandleConnectionAsync(clientId, socket, context.RequestAborted);
        });

        return endpoints;
    }
}
