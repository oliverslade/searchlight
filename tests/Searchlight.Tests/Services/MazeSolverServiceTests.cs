using Moq;
using Searchlight.Clients.Interfaces;
using Searchlight.Models;
using Searchlight.Services;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Searchlight.Tests.Services
{
    [TestClass]
    public class MazeSolverServiceTests
    {
        private Mock<IWebSocketWrapper> _mockWebSocketWrapper = null!;
        private MazeSolverService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockWebSocketWrapper = new Mock<IWebSocketWrapper>();
            _service = new MazeSolverService(_mockWebSocketWrapper.Object);
        }

        [TestMethod]
        public async Task SolveMazeAsync_WithValidMazeId_ReturnsSolveResult()
        {
            string mazeId = "testMazeId";

            var startLocation = new MazeLocation
            {
                Id = "START",
                AvailableDirections = new List<string>()
            };

            _mockWebSocketWrapper.Setup(ws => ws.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var json = JsonSerializer.Serialize(startLocation);
            var bytes = Encoding.UTF8.GetBytes(json);

            _mockWebSocketWrapper.Setup(ws => ws.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
                .Callback<ArraySegment<byte>, CancellationToken>((buffer, token) =>
                {
                    Array.Copy(bytes, 0, buffer.Array!, buffer.Offset, bytes.Length);
                })
                .ReturnsAsync(new WebSocketReceiveResult(
                    bytes.Length,
                    WebSocketMessageType.Text,
                    true));

            _mockWebSocketWrapper.Setup(ws => ws.SendAsync(
                It.IsAny<ArraySegment<byte>>(),
                WebSocketMessageType.Text,
                true,
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _service.SolveMazeAsync(mazeId);

            Assert.IsNotNull(result);
            _mockWebSocketWrapper.Verify(ws => ws.ConnectAsync(
                It.Is<Uri>(uri => uri.ToString().Contains(mazeId)),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SolveMazeAsync_WithEmptyMazeId_ThrowsArgumentException()
        {
            await _service.SolveMazeAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SolveMazeAsync_WithNullMazeId_ThrowsArgumentException()
        {
            // Act
            await _service.SolveMazeAsync(null!);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SolveMazeAsync_WithWhitespaceMazeId_ThrowsArgumentException()
        {
            // Act
            await _service.SolveMazeAsync("   ");
        }
    }
}