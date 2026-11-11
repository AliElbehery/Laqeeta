namespace Laqeeta.Core.Models;

/// <summary>Join table (many-to-many) between Station and MetroLine. Composite PK.</summary>
public class StationLine
{
    public long StationId { get; set; }
    public long LineId { get; set; }

    public Station Station { get; set; } = null!;
    public MetroLine Line { get; set; } = null!;
}