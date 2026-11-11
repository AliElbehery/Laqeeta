namespace Laqeeta.Core.Exceptions;

/// <summary>
/// Base of every business exception. <see cref="Code"/> is a stable key (see <c>Err</c>) that the web site and
/// the API translate to Arabic / English; <c>Message</c> stays an English fallback (logs, old callers).
/// </summary>
public abstract class DomainException : Exception
{
    public string? Code { get; }

    protected DomainException(string message, string? code) : base(message) => Code = code;
}

/// <summary>Entity not found => map to HTTP 404.</summary>
public class NotFoundException(string message, string? code = null) : DomainException(message, code);

/// <summary>Business rule violated => map to HTTP 400.</summary>
public class BusinessRuleException(string message, string? code = null) : DomainException(message, code);

/// <summary>User is not allowed to do this => map to HTTP 403.</summary>
public class ForbiddenException(string message, string? code = null) : DomainException(message, code);

/// <summary>Bad / missing credentials => map to HTTP 401.</summary>
public class UnauthorizedException(string message, string? code = null) : DomainException(message, code);

/// <summary>Duplicate resource (username/email taken) => map to HTTP 409.</summary>
public class ConflictException(string message, string? code = null) : DomainException(message, code);