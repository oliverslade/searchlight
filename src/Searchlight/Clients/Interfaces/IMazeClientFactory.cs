using System;
using System.Threading.Tasks;

namespace Searchlight.Clients.Interfaces
{
    public interface IMazeClientFactory
    {
        IMazeClient CreateClient(string mazeId);
    }
}