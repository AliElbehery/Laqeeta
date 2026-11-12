using Laqeeta.Core.DTOs;
using Laqeeta.Core.Models;

namespace Laqeeta.Core.Interfaces;

public interface ILostReportService
{
    Task<LostReport> CreateAsync(long userId, CreateLostReportDto dto, CancellationToken ct = default);

    Task<IReadOnlyList<LostReport>> GetByUserAsync(long userId, CancellationToken ct = default);
<<<<<<< HEAD

=======
    Task<(IReadOnlyList<LostReport> Items, LostReportCursor? NextCursor)> GetPageByUserAsync(
        long userId, LostReportCursor? after = null, CancellationToken ct = default);
>>>>>>> e2f7b660563f124ae3d557ed30a57e6a0b59274e
    Task<LostReport> GetByIdAsync(long reportId, long userId, CancellationToken ct = default);

    Task CancelAsync(long reportId, long userId, CancellationToken ct = default);
}