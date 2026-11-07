namespace Laqeeta.Core.Models;

public class Station
{
    public long StationId { get; set; }
    public string StationName { get; set; } = string.Empty;
    public string? LocationDetail { get; set; }
    public bool AcceptsItems { get; set; }

    public ICollection<StationLine> StationLines { get; set; } = new List<StationLine>();
    public ICollection<User> Employees { get; set; } = new List<User>();
    public ICollection<FoundItem> ItemsFoundHere { get; set; } = new List<FoundItem>();
    public ICollection<FoundItem> ItemsHeldHere { get; set; } = new List<FoundItem>();
}
