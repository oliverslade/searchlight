using Searchlight.Clients.Interfaces;

namespace Searchlight.Clients
{
    public class MazeClientFactory : IMazeClientFactory
    {
        public IMazeClient CreateClient(string mazeId)
        {
            return new MazeClient(mazeId);
        }
    }
}