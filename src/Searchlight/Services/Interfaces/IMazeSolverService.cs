using Searchlight.Models;
using System.Threading.Tasks;

namespace Searchlight.Services.Interfaces
{
    public interface IMazeSolverService
    {
        Task<SolveResult> SolveMazeAsync(string mazeId);
    }
}