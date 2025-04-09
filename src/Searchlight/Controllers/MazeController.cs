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
            return Ok(new { Message = $"Solving maze: {mazeId}" });
        }
    }
}
