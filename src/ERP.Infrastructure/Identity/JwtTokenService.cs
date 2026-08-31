using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ERP.Domain.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ERP.Infrastructure.Identity;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GerarToken(
        string usuarioId,
        string email,
        string nomeCompleto,
        int empresaAtualId,
        IEnumerable<int> empresaIds,
        IEnumerable<string> permissoes)
    {
        var secret = _configuration["JwtSettings:Secret"] ?? "ERP_Super_Secret_Key_2026_Enterprise_System_Token_Secret!";
        var expiryMinutesStr = _configuration["JwtSettings:ExpiryMinutes"] ?? "60";
        double.TryParse(expiryMinutesStr, out var expiryMinutes);
        if (expiryMinutes <= 0) expiryMinutes = 60;

        var key = Encoding.ASCII.GetBytes(secret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, nomeCompleto),
            new Claim("EmpresaId", empresaAtualId.ToString())
        };

        foreach (var empId in empresaIds)
        {
            claims.Add(new Claim("PermittedEmpresaId", empId.ToString()));
        }

        foreach (var perm in permissoes)
        {
            claims.Add(new Claim("Permission", perm));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string GerarRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public (bool Valido, string? UsuarioId) ValidarToken(string token)
    {
        var secret = _configuration["JwtSettings:Secret"] ?? "ERP_Super_Secret_Key_2026_Enterprise_System_Token_Secret!";
        var key = Encoding.ASCII.GetBytes(secret);

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var usuarioId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return (usuarioId != null, usuarioId);
        }
        catch
        {
            return (false, null);
        }
    }
}
