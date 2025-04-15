using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Searchlight.Clients.Interfaces;
using Searchlight.Controllers;
using Searchlight.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Searchlight.Tests.Controllers
{
    [TestClass]
    public class MazeControllerTests
    {
        private Mock<IMazeClientFactory> _mockClientFactory = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockClientFactory = new Mock<IMazeClientFactory>();
        }

        [TestMethod]
        public void SolveMaze_WithNullOrEmptyMazeId_ReturnsBadRequest()
        {
            var controller = new MazeController(_mockClientFactory.Object);

            var result = controller.SolveMaze("").Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }
    }
}
