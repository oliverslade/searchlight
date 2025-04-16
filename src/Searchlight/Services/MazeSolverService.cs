using Searchlight.Clients;
using Searchlight.Clients.Interfaces;
using Searchlight.Models;
using Searchlight.Services.Interfaces;

namespace Searchlight.Services
{
    public class MazeSolverService : IMazeSolverService
    {
        private readonly IWebSocketWrapper _webSocketWrapper;

        public MazeSolverService(IWebSocketWrapper webSocketWrapper)
        {
            _webSocketWrapper = webSocketWrapper;
        }

        public async Task<SolveResult> SolveMazeAsync(string mazeId)
        {
            if (string.IsNullOrWhiteSpace(mazeId))
            {
                throw new ArgumentException("Maze ID is required", nameof(mazeId));
            }

            using var mazeClient = new MazeClient(mazeId, _webSocketWrapper);

            var mazeSolver = new MazeSolver(mazeClient);

            return await mazeSolver.SolveAsync();
        }
    }
}
