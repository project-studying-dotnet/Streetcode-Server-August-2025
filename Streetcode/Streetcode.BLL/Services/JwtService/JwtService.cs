using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Streetcode.BLL.Services.JwtService;

public class JwtService : IJwtService
{
    private readonly JwtEnvironmentVariables _jwtVariables;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly StreetcodeDbContext _dbContext;
    private readonly SigningCredentials _signingCredentials;

    public JwtService(IConfiguration configuration, StreetcodeDbContext dbContext)
    {
        _jwtVariables = configuration
            .GetSection("JwtSettings")
            .Get<JwtEnvironmentVariables>()!;

        _dbContext = dbContext;

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtVariables.SecretKey));
        _signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    }

    public async Task<string?> GenerateTokenAsync(int userId, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            u => u.Id == userId, cancellationToken: ct);

        if (user is null)
        {
            throw new KeyNotFoundException("User with this userId was not found");
        }

        var descriptor = GetTokenDescriptor(user);
        var token = _jwtSecurityTokenHandler.CreateToken(descriptor);
        var jwt = _jwtSecurityTokenHandler.WriteToken(token);

        return jwt;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var key = Encoding.UTF8.GetBytes(_jwtVariables.SecretKey);

        try
        {
            var principal = _jwtSecurityTokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                ValidateIssuer = true,
                ValidIssuer = _jwtVariables.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtVariables.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
                },
                out _);

            return principal;
        }
        catch
        {
            // invalid token
            return null;
        }
    }

    public int? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null)
        {
            return null;
        }

        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return null;
        }

        if (int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    private SecurityTokenDescriptor GetTokenDescriptor(User user)
    {
        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.GivenName, user.Name),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Surname, user.Surname),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtVariables.ExpiryMinutes),
            SigningCredentials = _signingCredentials,
            Issuer = _jwtVariables.Issuer,
            Audience = _jwtVariables.Audience
        };
    }
}