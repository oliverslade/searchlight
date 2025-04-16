using Searchlight.Models;

namespace Searchlight.Services.Interfaces
{
    public interface IMazeSolver
    {
        Task<SolveResult> SolveAsync();
    }
}
