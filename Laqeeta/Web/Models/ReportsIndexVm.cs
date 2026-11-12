using Laqeeta.Core.Models;

namespace Laqeeta.Api.Web.Models;

public class ReportsIndexVm
{
    public IReadOnlyList<LostReport> Reports { get; set; } = [];
    public string? NextCursor { get; set; }
}
