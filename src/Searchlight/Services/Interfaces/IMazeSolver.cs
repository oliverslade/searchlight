using Searchlight.Models;
using System.Threading.Tasks;

namespace Searchlight.Services.Interfaces
{
    public interface IMazeSolver
    {
        Task<SolveResult> SolveAsync();
    }
}