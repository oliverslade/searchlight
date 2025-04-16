namespace Searchlight.Models
{
    public class NodeInfo
    {
        public string Id { get; set; } = string.Empty;
        public string? ParentId { get; set; }
        public string? EntryDirection { get; set; }
    }
}