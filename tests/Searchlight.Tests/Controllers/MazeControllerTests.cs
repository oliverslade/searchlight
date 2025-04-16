using Microsoft.AspNetCore.Mvc;
using Moq;
using Searchlight.Controllers;
using Searchlight.Models;
using Searchlight.Services.Interfaces;
using System.Text.Json;

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

            var badRequestResult = result as BadRequestObjectResult;
            var responseValue = badRequestResult?.Value;
            Assert.IsNotNull(responseValue);

            var json = JsonSerializer.Serialize(responseValue);
            var responseDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            Assert.IsNotNull(responseDict);
            Assert.IsTrue(responseDict.ContainsKey("Error"));
            Assert.AreEqual("Maze ID is required", responseDict["Error"].GetString());
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

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var jsonResponse = JsonSerializer.Serialize(okResult.Value);
            var responseObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonResponse);

            Assert.IsNotNull(responseObject);

            Assert.IsTrue(responseObject.ContainsKey("MazeId"));
            Assert.IsTrue(responseObject.ContainsKey("DreamJob"));
            Assert.IsTrue(responseObject.ContainsKey("SolvedIn"));
            Assert.IsTrue(responseObject.ContainsKey("TotalMoves"));
            Assert.IsTrue(responseObject.ContainsKey("TotalResets"));

            Assert.AreEqual(mazeId, responseObject["MazeId"].GetString());
            Assert.AreEqual("Test Engineer at Test Company", responseObject["DreamJob"].GetString());
            Assert.AreEqual(42, responseObject["TotalMoves"].GetInt32());
            Assert.AreEqual(0, responseObject["TotalResets"].GetInt32());

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

            var notFoundResult = result as NotFoundObjectResult;
            var responseValue = notFoundResult?.Value;
            Assert.IsNotNull(responseValue);

            var json = JsonSerializer.Serialize(responseValue);
            var responseDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            Assert.IsNotNull(responseDict);
            Assert.IsTrue(responseDict.ContainsKey("Error"));
            Assert.AreEqual("Could not find a solution for this maze", responseDict["Error"].GetString());
        }

        [TestMethod]
        public async Task SolveMaze_WhenExceptionIsThrown_ReturnsInternalServerError()
        {
            var mazeId = "exceptionMazeId";
            var expectedErrorMessage = "Test exception";

            _mockSolverService
                .Setup(s => s.SolveMazeAsync(mazeId))
                .ThrowsAsync(new Exception(expectedErrorMessage));

            var controller = new MazeController(_mockSolverService.Object);

            var result = await controller.SolveMaze(mazeId);

            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var statusCodeResult = (ObjectResult)result;
            Assert.AreEqual(500, statusCodeResult.StatusCode);

            var responseValue = statusCodeResult.Value;
            Assert.IsNotNull(responseValue);

            var json = JsonSerializer.Serialize(responseValue);
            var responseDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            Assert.IsNotNull(responseDict);
            Assert.IsTrue(responseDict.ContainsKey("Error") &&
                          responseDict["Error"].GetString() != null &&
                          responseDict["Error"].GetString()!.Contains(expectedErrorMessage));
        }
    }
}
