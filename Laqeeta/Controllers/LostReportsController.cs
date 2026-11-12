using System.Buffers.Binary;
using System.Buffers.Text;
using Laqeeta.Api.Auth;
using Laqeeta.Api.Mapping;
using Laqeeta.Core.DTOs;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Laqeeta.Api.Controllers;

/// <summary>Passenger side: lost reports and the matches found for them.</summary>
[ApiController]
[Route("api/lost-reports")]
[Authorize(Policy = "PassengerOnly")]
public class LostReportsController(ILostReportService reports, IMatchingService matching) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<LostReportDto>> Create(CreateLostReportDto dto, CancellationToken ct)
    {
        var created = await reports.CreateAsync(User.GetUserId(), dto, ct);
        var full = await reports.GetByIdAsync(created.ReportId, User.GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = created.ReportId }, full.ToDto());
    }

    /// <summary>My reports, newest first, up to 10 at a time.</summary>
    /// <param name="cursor">The previous response's nextCursor. Omit for the first page.</param>
    /// <param name="ct">Request cancellation token.</param>
    [HttpGet]
    public async Task<ActionResult<LostReportPageDto>> GetMine([FromQuery] string? cursor, CancellationToken ct)
    {
        LostReportCursor? after = null;
        if ((cursor is not null || Request.Query.ContainsKey(nameof(cursor))) && !TryDecodeCursor(cursor, out after))
        {
            ModelState.AddModelError(nameof(cursor), "Use the nextCursor from a previous response.");
            return ValidationProblem(ModelState);
        }

        var (items, nextCursor) = await reports.GetPageByUserAsync(User.GetUserId(), after, ct);
        return new LostReportPageDto(items.Select(report => report.ToDto()).ToArray(),
            nextCursor is { } next ? EncodeCursor(next) : null);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<LostReportDto>> GetById(long id, CancellationToken ct)
        => (await reports.GetByIdAsync(id, User.GetUserId(), ct)).ToDto();

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id, CancellationToken ct)
    {
        await reports.CancelAsync(id, User.GetUserId(), ct);
        return NoContent();
    }

    /// <summary>Possible matches for my report (private identifying details are never included).</summary>
    [HttpGet("{id:long}/matches")]
    public async Task<ActionResult<IEnumerable<PassengerMatchDto>>> GetMatches(long id, CancellationToken ct)
        => Ok((await matching.GetForReportAsync(id, User.GetUserId(), ct)).Select(m => m.ToPassengerDto()));


    private static string EncodeCursor(LostReportCursor cursor)
    {
        Span<byte> bytes = stackalloc byte[2 * sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(bytes, cursor.CreatedAt.Ticks);
        BinaryPrimitives.WriteInt64BigEndian(bytes[sizeof(long)..], cursor.ReportId);
        return Base64Url.EncodeToString(bytes);
    }

    private static bool TryDecodeCursor(string? cursor, out LostReportCursor? after)
    {
        after = null;
        Span<byte> bytes = stackalloc byte[2 * sizeof(long)];
        if (cursor is null || cursor.Length != Base64Url.GetEncodedLength(bytes.Length) ||
            !Base64Url.TryDecodeFromChars(cursor, bytes, out var written) || written != bytes.Length)
        {
            return false;
        }

        var ticks = BinaryPrimitives.ReadInt64BigEndian(bytes);
        var reportId = BinaryPrimitives.ReadInt64BigEndian(bytes[sizeof(long)..]);
        if (ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks || reportId <= 0)
            return false;

        after = new LostReportCursor(new DateTime(ticks, DateTimeKind.Utc), reportId);
        return true;
    }
}
