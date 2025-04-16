using Searchlight.Models;

namespace Searchlight.Services.Interfaces
{
    public interface IMazeSolverService
    {
        Task<SolveResult> SolveMazeAsync(string mazeId);
    }
}
