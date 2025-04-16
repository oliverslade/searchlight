using Searchlight.Clients;
using Searchlight.Clients.Interfaces;
using Searchlight.Models;
using Searchlight.Services.Interfaces;
using System;
using System.Threading.Tasks;

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

            // Create the client per request with the mazeId
            using var mazeClient = new MazeClient(mazeId, _webSocketWrapper);

            // Create the solver with the client
            var mazeSolver = new MazeSolver(mazeClient);

            // Solve the maze
            return await mazeSolver.SolveAsync();
        }
    }
}