using System.Net.WebSockets;

namespace Searchlight.Clients.Interfaces
{
    public interface IWebSocketWrapper : IDisposable
    {
        Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
        
        Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
        
        Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);

        Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken);
    }
}
