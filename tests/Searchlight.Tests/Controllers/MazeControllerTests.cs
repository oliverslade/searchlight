using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Searchlight.Controllers;
using Searchlight.Models;
using Searchlight.Services.Interfaces;
using System;
using System.Threading.Tasks;

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
            // Arrange
            var controller = new MazeController(_mockSolverService.Object);

            // Act
            var result = controller.SolveMaze("").Result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task SolveMaze_WithValidMazeId_ReturnsOkResult()
        {
            // Arrange
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

            // Act
            var result = await controller.SolveMaze(mazeId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _mockSolverService.Verify(s => s.SolveMazeAsync(mazeId), Times.Once);
        }

        [TestMethod]
        public async Task SolveMaze_WhenSolvingFails_ReturnsNotFoundResult()
        {
            // Arrange
            var mazeId = "unsolvableMazeId";
            var solveResult = new SolveResult
            {
                Success = false
            };

            _mockSolverService
                .Setup(s => s.SolveMazeAsync(mazeId))
                .ReturnsAsync(solveResult);

            var controller = new MazeController(_mockSolverService.Object);

            // Act
            var result = await controller.SolveMaze(mazeId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task SolveMaze_WhenExceptionIsThrown_ReturnsInternalServerError()
        {
            // Arrange
            var mazeId = "exceptionMazeId";

            _mockSolverService
                .Setup(s => s.SolveMazeAsync(mazeId))
                .ThrowsAsync(new Exception("Test exception"));

            var controller = new MazeController(_mockSolverService.Object);

            // Act
            var result = await controller.SolveMaze(mazeId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var statusCodeResult = (ObjectResult)result;
            Assert.AreEqual(500, statusCodeResult.StatusCode);
        }
    }
}
