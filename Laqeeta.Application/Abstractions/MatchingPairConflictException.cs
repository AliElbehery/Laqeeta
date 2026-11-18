namespace Laqeeta.Application.Abstractions;

public sealed class MatchingPairConflictException : Exception
{
    public MatchingPairConflictException(Exception innerException)
        : base("A matching pair was inserted by another request.", innerException)
    {
    }
}