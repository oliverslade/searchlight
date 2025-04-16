using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Moq;
using Searchlight.Models;
using Searchlight.Clients;
using Searchlight.Clients.Interfaces;

namespace Searchlight.Tests.Clients
{
    [TestClass]
    public class MazeClientTests
    {
        private Mock<IWebSocketWrapper> _mockWebSocket = null!;
        private MazeClient _client = null!;
        private readonly string _testMazeId = "TEST123456789";

        [TestInitialize]
        public void SetUp()
        {
            _mockWebSocket = new Mock<IWebSocketWrapper>();
            _client = new MazeClient(_testMazeId, _mockWebSocket.Object);
        }

        [TestMethod]
        public async Task ConnectAsync_WhenNotConnected_ConnectsToWebSocket()
        {
            _mockWebSocket.Setup(ws => ws.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _client.ConnectAsync();

            _mockWebSocket.Verify(ws => ws.ConnectAsync(
                It.Is<Uri>(uri => uri.ToString() == $"wss://maze.robanderson.dev/ws/{_testMazeId}"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public async Task ConnectAsync_WhenAlreadyConnected_DoesNotConnectAgain()
        {
            _mockWebSocket.Setup(ws => ws.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await _client.ConnectAsync();

            await _client.ConnectAsync();

            _mockWebSocket.Verify(ws => ws.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ReceiveLocationAsync_DeserializesLocationCorrectly()
        {
            var expectedLocation = new MazeLocation
            {
                Id = "LOC123",
                Name = "TestCompany",
                Title = "Developer",
                Description = "A test job",
                AvailableDirections = new List<string> { Direction.Up, Direction.Left }
            };

            var jsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(expectedLocation));

            _mockWebSocket.Setup(ws => ws.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockWebSocket.Setup(ws => ws.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Callback<ArraySegment<byte>, CancellationToken>((buffer, token) =>
                {
                    Array.Copy(jsonBytes, 0, buffer.Array!, buffer.Offset, jsonBytes.Length);
                })
                .ReturnsAsync(new WebSocketReceiveResult(
                    jsonBytes.Length,
                    WebSocketMessageType.Text,
                    true));

            var result = await _client.ReceiveLocationAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedLocation.Id, result.Id);
            Assert.AreEqual(expectedLocation.Name, result.Name);
            Assert.AreEqual(expectedLocation.Title, result.Title);
            Assert.AreEqual(expectedLocation.Description, result.Description);
            CollectionAssert.AreEqual(expectedLocation.AvailableDirections, result.AvailableDirections);
        }

        [TestMethod]
        public async Task SendCommandAsync_SerializesCommandCorrectly()
        {
            var command = MazeCommand.Go(Direction.Up);
            var expectedJson = JsonSerializer.Serialize(command);
            byte[]? capturedData = null;

            _mockWebSocket.Setup(ws => ws.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockWebSocket.Setup(ws => ws.SendAsync(
                    It.IsAny<ArraySegment<byte>>(),
                    WebSocketMessageType.Text,
                    true,
                    It.IsAny<CancellationToken>()))
                .Callback<ArraySegment<byte>, WebSocketMessageType, bool, CancellationToken>(
                    (data, messageType, endOfMessage, token) =>
                    {
                        capturedData = data.ToArray();
                    })
                .Returns(Task.CompletedTask);

            await _client.SendCommandAsync(command);

            Assert.IsNotNull(capturedData);
            var actualJson = Encoding.UTF8.GetString(capturedData);
            Assert.AreEqual(expectedJson, actualJson);
        }

        [TestMethod]
        public async Task MoveAsync_SendsGoCommandAndReturnsLocation()
        {
            var direction = Direction.Right;
            var expectedLocation = new MazeLocation
            {
                Id = "LOC456",
                AvailableDirections = new List<string> { Direction.Up, Direction.Right }
            };

            var jsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(expectedLocation));

            _mockWebSocket.Setup(ws => ws.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockWebSocket.Setup(ws => ws.SendAsync(
                    It.IsAny<ArraySegment<byte>>(),
                    WebSocketMessageType.Text,
                    true,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockWebSocket.Setup(ws => ws.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Callback<ArraySegment<byte>, CancellationToken>((buffer, token) =>
                {
                    Array.Copy(jsonBytes, 0, buffer.Array!, buffer.Offset, jsonBytes.Length);
                })
                .ReturnsAsync(new WebSocketReceiveResult(
                    jsonBytes.Length,
                    WebSocketMessageType.Text,
                    true));

            var result = await _client.MoveAsync(direction);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedLocation.Id, result.Id);
            _mockWebSocket.Verify(ws => ws.SendAsync(
                It.IsAny<ArraySegment<byte>>(),
                WebSocketMessageType.Text,
                true,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ResetAsync_SendsResetCommandAndReturnsLocation()
        {
            var expectedLocation = new MazeLocation
            {
                Id = "START",
                AvailableDirections = new List<string> { Direction.Down, Direction.Right }
            };

            var jsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(expectedLocation));

            _mockWebSocket.Setup(ws => ws.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockWebSocket.Setup(ws => ws.SendAsync(
                    It.IsAny<ArraySegment<byte>>(),
                    WebSocketMessageType.Text,
                    true,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockWebSocket.Setup(ws => ws.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Callback<ArraySegment<byte>, CancellationToken>((buffer, token) =>
                {
                    Array.Copy(jsonBytes, 0, buffer.Array!, buffer.Offset, jsonBytes.Length);
                })
                .ReturnsAsync(new WebSocketReceiveResult(
                    jsonBytes.Length,
                    WebSocketMessageType.Text,
                    true));

            var result = await _client.ResetAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedLocation.Id, result.Id);
            _mockWebSocket.Verify(ws => ws.SendAsync(
                It.IsAny<ArraySegment<byte>>(),
                WebSocketMessageType.Text,
                true,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public void Dispose_DisposesWebSocketAndCancellationTokenSource()
        {
            _mockWebSocket.Setup(ws => ws.Dispose());

            _client.Dispose();

            _mockWebSocket.Verify(ws => ws.Dispose(), Times.Once);
        }
    }
}
