using Microsoft.AspNetCore.Mvc;
using Searchlight.Controllers;
using System.Text.Json;

namespace Searchlight.Tests.Controllers
{
    [TestClass]
    public class HealthControllerTests
    {
        [TestMethod]
        public void Check_ReturnsOkResultWithHealthyStatus()
        {
            var controller = new HealthController();

            var result = controller.Check();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "okResult is null.");
            Assert.AreEqual(200, okResult!.StatusCode);

            var responseValue = okResult.Value;
            var jsonString = JsonSerializer.Serialize(responseValue);
            var responseObject = JsonSerializer.Deserialize<HealthResponse>(jsonString);

            Assert.IsNotNull(responseObject, "Deserialized responseObject is null.");
            Assert.AreEqual("healthy", responseObject.status);
            Assert.IsTrue(DateTime.UtcNow.Subtract(responseObject.timestamp).TotalSeconds < 5);
        }

        private class HealthResponse
        {
            public string? status { get; set; }
            public DateTime timestamp { get; set; }
        }
    }
}
