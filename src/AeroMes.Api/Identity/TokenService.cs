using AeroMes.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AeroMes.Api.Identity;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string CreateToken(
        string userId, string email, IEnumerable<string> roles,
        int? workCenterId = null, bool mfaVerified = false, string? preferredLanguage = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        if (workCenterId.HasValue)
            claims.Add(new Claim("wc_scope", workCenterId.Value.ToString()));

        if (mfaVerified)
            claims.Add(new Claim("mfa_verified", "true"));

        if (preferredLanguage is not null)
            claims.Add(new Claim("preferred_language", preferredLanguage));

        var expiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var m) ? m : 15;
        return BuildToken(claims, expiryMinutes);
    }

    public string CreateMfaPendingToken(string userId, string email)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("mfa_pending", "true"),
        };
        return BuildToken(claims, expiryMinutes: 5);
    }

    private string BuildToken(IEnumerable<Claim> claims, int expiryMinutes)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured.")));

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
