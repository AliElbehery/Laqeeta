namespace Laqeeta.Core.Models;

public class MetroLine
{
    public long LineId { get; set; }
    public string LineName { get; set; } = string.Empty;

    public ICollection<StationLine> StationLines { get; set; } = new List<StationLine>();
    public ICollection<LostReport> LostReports { get; set; } = new List<LostReport>();
}