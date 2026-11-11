namespace Laqeeta.Core.Models;

/// <summary>
/// Optimistic-concurrency marker. <c>AppDbContext</c> gives it a new value on every insert/update, and every
/// UPDATE is sent as "... WHERE id = @id AND concurrency_stamp = @valueWeRead", so when two requests change the
/// same row only the first one wins and the second one gets a 409 instead of silently overwriting it.
/// </summary>
public interface IHasConcurrencyStamp
{
    Guid ConcurrencyStamp { get; set; }
}