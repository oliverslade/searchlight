using System.Net.WebSockets;

namespace Searchlight.Services
{
    /// <summary>
    /// Wrapper interface for WebSocket operations to enable unit testing
    /// </summary>
    public interface IWebSocketWrapper : IDisposable
    {
        /// <summary>
        /// Connects to a WebSocket endpoint
        /// </summary>
        Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
        
        /// <summary>
        /// Sends data over the WebSocket
        /// </summary>
        Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
        
        /// <summary>
        /// Receives data from the WebSocket
        /// </summary>
        Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
        
        /// <summary>
        /// Closes the WebSocket connection
        /// </summary>
        Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken);
    }
}
