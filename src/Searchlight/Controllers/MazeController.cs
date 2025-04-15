using Microsoft.AspNetCore.Mvc;
using Searchlight.Clients.Interfaces;
using Searchlight.Services;
using System;
using System.Threading.Tasks;

namespace Searchlight.Controllers
{
    [ApiController]
    [Route("api/mazes")]
    public class MazeController : ControllerBase
    {
        private readonly IMazeClientFactory _mazeClientFactory;

        public MazeController(IMazeClientFactory mazeClientFactory)
        {
            _mazeClientFactory = mazeClientFactory;
        }

        [HttpGet("{mazeId}/solution")]
        public async Task<IActionResult> SolveMaze(string mazeId)
        {
            if (string.IsNullOrWhiteSpace(mazeId))
            {
                return BadRequest(new { Error = "Maze ID is required" });
            }

            try
            {
                using var mazeClient = _mazeClientFactory.CreateClient(mazeId);
                var mazeSolver = new MazeSolver(mazeClient);

                var startTime = DateTime.UtcNow;
                var result = await mazeSolver.SolveAsync();
                var endTime = DateTime.UtcNow;
                var duration = endTime - startTime;

                if (!result.Success)
                {
                    return NotFound(new { Error = "Could not find a solution for this maze" });
                }

                var response = new
                {
                    Success = result.Success,
                    MazeId = mazeId,
                    DreamJob = result.EndLocation != null
                        ? $"{result.EndLocation.Title} at {result.EndLocation.Name}"
                        : "Unknown Dream Job",
                    SolvedIn = $"{duration.TotalSeconds:F2} seconds",
                    TotalMoves = result.TotalMoves,
                    TotalResets = result.TotalResets
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to solve maze: {ex.Message}" });
            }
        }
    }
}
