using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Searchlight.Controllers;
using System.Text.Json;

namespace Searchlight.Tests.Controllers
{
    [TestClass]
    public class MazeControllerTests
    {
        [TestMethod]
        public void SolveMaze_ReturnsOkResult()
        {
            var controller = new MazeController();
            var mazeId = "abc123";

            var result = controller.SolveMaze(mazeId);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "okResult is null.");
            Assert.AreEqual(200, okResult!.StatusCode);

            var responseValue = okResult.Value;
            var jsonString = JsonSerializer.Serialize(responseValue);
            var responseObject = JsonSerializer.Deserialize<SolveResponse>(jsonString);

            Assert.IsNotNull(responseObject, "Deserialized responseObject is null.");
            Assert.AreEqual($"Solving maze: {mazeId}", responseObject.message);
        }

        private class SolveResponse
        {
            public string? message { get; set; }
        }
    }
}
