using System.Text.Json.Serialization;

namespace Searchlight.Models
{
    public class MazeLocation
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("availableDirections")]
        public List<string> AvailableDirections { get; set; } = new List<string>();

        [JsonIgnore]
        public bool IsEndOfMaze => AvailableDirections.Count == 0;

        public bool CanMove(string direction)
        {
            return AvailableDirections.Contains(direction);
        }
    }
}
