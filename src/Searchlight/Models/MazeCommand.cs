using System.Text.Json.Serialization;

namespace Searchlight.Models
{
    public class MazeCommand
    {
        [JsonPropertyName("command")]
        public string Command { get; set; } = string.Empty;

        public static MazeCommand Go(string direction)
        {
            return new MazeCommand { Command = $"go {direction}" };
        }

        public static MazeCommand Reset()
        {
            return new MazeCommand { Command = "reset" };
        }
    }
}
