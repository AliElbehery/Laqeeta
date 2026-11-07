using System.Text;
using Laqeeta.Core.Interfaces;
using Laqeeta.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Laqeeta.Api.Auth;

public class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private static readonly JsonWebTokenHandler Handler = new();

    public TokenResult Create(User user)
    {
        var o = options.Value;
        var expires = DateTime.UtcNow.AddMinutes(o.ExpiryMinutes);

        var claims = new Dictionary<string, object>
        {
            [ClaimNames.UserId] = user.UserId.ToString(),
            [ClaimNames.Name]   = user.Username,
            [ClaimNames.Role]   = user.Role.ToString()
        };
        if (user.StationId is not null)
            claims[ClaimNames.StationId] = user.StationId.Value.ToString();

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = o.Issuer,
            Audience = o.Audience,
            Expires = expires,
            Claims = claims,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.Key)), SecurityAlgorithms.HmacSha256)
        };

        return new TokenResult(Handler.CreateToken(descriptor), expires);
    }
}
