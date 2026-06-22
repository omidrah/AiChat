using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AiChat.Application.Common.Auth;
using Microsoft.IdentityModel.Tokens;

namespace AiChat.Api.Services;

public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(
        string userId,
        string userName,
        IEnumerable<string> roles)
    {
        var secret = _config["Authentication:Jwt:Secret"];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userName)
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: _config["Authentication:Jwt:Issuer"],
            audience: _config["Authentication:Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
