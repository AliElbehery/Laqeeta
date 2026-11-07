using Laqeeta.Core.Models;

namespace Laqeeta.Core.Interfaces;

public record TokenResult(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    TokenResult Create(User user);
}
