namespace Searchlight.Models
{
    public class MazeNode
    {
        public List<string> AvailableDirections { get; set; } = new List<string>();
        public Dictionary<string, string> Neighbours { get; set; } = new Dictionary<string, string>();
        public MazeLocation Data { get; set; } = new MazeLocation();
    }
}
