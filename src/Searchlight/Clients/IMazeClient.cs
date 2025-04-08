using Searchlight.Models;

namespace Searchlight.Services
{
    public interface IMazeClient : IDisposable
    {
        Task ConnectAsync();
        Task<MazeLocation> ReceiveLocationAsync();
        Task SendCommandAsync(MazeCommand command);
        Task<MazeLocation> MoveAsync(string direction);
        Task<MazeLocation> ResetAsync();
    }
}
