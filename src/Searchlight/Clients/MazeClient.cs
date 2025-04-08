using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Searchlight.Models;
using Searchlight.Clients.Interfaces;

namespace Searchlight.Clients
{
    public class MazeClient : IMazeClient, IDisposable
    {
        private readonly IWebSocketWrapper _webSocket;
        private readonly string _mazeId;
        private readonly Uri _mazeUri;
        private readonly CancellationTokenSource _cancellationSource;
        private bool _isConnected;

        public MazeClient(string mazeId) : this(mazeId, new ClientWebSocketWrapper())
        {
        }

        public MazeClient(string mazeId, IWebSocketWrapper webSocket)
        {
            _mazeId = mazeId ?? throw new ArgumentNullException(nameof(mazeId));
            _mazeUri = new Uri($"wss://maze.robanderson.dev/ws/{_mazeId}");
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _cancellationSource = new CancellationTokenSource();
        }

        public async Task ConnectAsync()
        {
            if (_isConnected)
                return;

            await _webSocket.ConnectAsync(_mazeUri, _cancellationSource.Token);
            _isConnected = true;
        }

        public async Task<MazeLocation> ReceiveLocationAsync()
        {
            if (!_isConnected)
                await ConnectAsync();

            var buffer = new byte[4096];
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationSource.Token);
            
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by server", _cancellationSource.Token);
                _isConnected = false;
                throw new InvalidOperationException("WebSocket connection was closed by the server");
            }

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var location = JsonSerializer.Deserialize<MazeLocation>(message);
            
            return location ?? throw new InvalidOperationException("Failed to parse maze location data");
        }

        public async Task SendCommandAsync(MazeCommand command)
        {
            if (!_isConnected)
                await ConnectAsync();

            var commandJson = JsonSerializer.Serialize(command);
            var buffer = Encoding.UTF8.GetBytes(commandJson);
            
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationSource.Token);
        }

        public async Task<MazeLocation> MoveAsync(string direction)
        {
            await SendCommandAsync(MazeCommand.Go(direction));
            return await ReceiveLocationAsync();
        }

        public async Task<MazeLocation> ResetAsync()
        {
            await SendCommandAsync(MazeCommand.Reset());
            return await ReceiveLocationAsync();
        }
        
        public void Dispose()
        {
            _cancellationSource.Cancel();
            _webSocket.Dispose();
            _cancellationSource.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
