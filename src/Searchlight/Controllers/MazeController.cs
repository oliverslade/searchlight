using Microsoft.AspNetCore.Mvc;

namespace Searchlight.Controllers
{
    [ApiController]
    [Route("api/maze")]
    public class MazeController : ControllerBase
    {
        [HttpPost("solve/{mazeId}")]
        public IActionResult SolveMaze(string mazeId)
        {
            return Ok(new { message = $"Solving maze: {mazeId}" });
        }
    }
}
