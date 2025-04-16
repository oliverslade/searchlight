namespace Searchlight.Models
{
    public class SolveResult
    {
        public bool Success { get; set; }
        public MazeLocation StartLocation { get; set; } = new MazeLocation();
        public MazeLocation? EndLocation { get; set; }
        public int TotalMoves { get; set; }
        public int TotalResets { get; set; }
        public int TotalLocationsDiscovered { get; set; }
        public List<MazeLocation> Path { get; set; } = new List<MazeLocation>();
    }
}
