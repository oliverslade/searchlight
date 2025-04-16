using Microsoft.AspNetCore.Mvc;
using Moq;
using Searchlight.Controllers;
using Searchlight.Models;
using Searchlight.Services.Interfaces;

namespace Searchlight.Tests.Controllers
{
    [TestClass]
    public class MazeControllerTests
    {
        private Mock<IMazeSolverService> _mockSolverService = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockSolverService = new Mock<IMazeSolverService>();
        }

        [TestMethod]
        public void SolveMaze_WithNullOrEmptyMazeId_ReturnsBadRequest()
        {
            var controller = new MazeController(_mockSolverService.Object);

            var result = controller.SolveMaze("").Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task SolveMaze_WithValidMazeId_ReturnsOkResult()
        {
            var mazeId = "validMazeId";
            var solveResult = new SolveResult
            {
                Success = true,
                TotalMoves = 42,
                TotalResets = 0,
                EndLocation = new MazeLocation
                {
                    Title = "Test Engineer",
                    Name = "Test Company"
                }
            };

            _mockSolverService
                .Setup(s => s.SolveMazeAsync(mazeId))
                .ReturnsAsync(solveResult);

            var controller = new MazeController(_mockSolverService.Object);

            var result = await controller.SolveMaze(mazeId);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockSolverService.Verify(s => s.SolveMazeAsync(mazeId), Times.Once);
        }

        [TestMethod]
        public async Task SolveMaze_WhenSolvingFails_ReturnsNotFoundResult()
        {
            var mazeId = "unsolvableMazeId";
            var solveResult = new SolveResult
            {
                Success = false
            };

            _mockSolverService
                .Setup(s => s.SolveMazeAsync(mazeId))
                .ReturnsAsync(solveResult);

            var controller = new MazeController(_mockSolverService.Object);

            var result = await controller.SolveMaze(mazeId);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task SolveMaze_WhenExceptionIsThrown_ReturnsInternalServerError()
        {
            var mazeId = "exceptionMazeId";

            _mockSolverService
                .Setup(s => s.SolveMazeAsync(mazeId))
                .ThrowsAsync(new Exception("Test exception"));

            var controller = new MazeController(_mockSolverService.Object);

            var result = await controller.SolveMaze(mazeId);

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var statusCodeResult = (ObjectResult)result;
            Assert.AreEqual(500, statusCodeResult.StatusCode);
        }
    }
}
