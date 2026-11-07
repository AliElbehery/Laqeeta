namespace Laqeeta.Core.Exceptions;

/// <summary>Entity not found => map to HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Business rule violated => map to HTTP 400/409.</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

/// <summary>User is not allowed to do this => map to HTTP 403.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

/// <summary>Bad / missing credentials => map to HTTP 401.</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

/// <summary>Duplicate resource (username/email taken) => map to HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
