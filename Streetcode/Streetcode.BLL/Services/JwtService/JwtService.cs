using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using Streetcode.BLL.DTO.Users;

namespace Streetcode.BLL.Services.JwtService;

public class JwtService : IJwtService
{
    private readonly JwtEnvironmentVariables _jwtVariables;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly StreetcodeDbContext _dbContext;
    private readonly SigningCredentials _signingCredentials;
    private readonly IMapper _mapper;

    public JwtService(IConfiguration configuration, StreetcodeDbContext dbContext, IMapper mapper)
    {
        _jwtVariables = configuration
            .GetSection("JwtSettings")
            .Get<JwtEnvironmentVariables>()!;

        _dbContext = dbContext;
        _mapper = mapper;

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtVariables.SecretKey));
        _signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    }

    public async Task<LoginResultDTO> GenerateTokenAsync(int userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            u => u.Id == userId);

        if (user is null)
        {
            throw new KeyNotFoundException("User with this userId was not found");
        }

        // Create JWT access token
        var descriptor = GetTokenDescriptor(user);
        var token = _jwtSecurityTokenHandler.CreateToken(descriptor);
        var jwt = _jwtSecurityTokenHandler.WriteToken(token);

        var expireAt = token.ValidTo;

        // Create refresh token
        var refreshToken = GenerateRefreshToken();
        var refreshExpiryDate = DateTime.UtcNow.AddDays(7);
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshExpiryDate;

        await _dbContext.SaveChangesAsync();
        var userDto = _mapper.Map<UserDTO>(user);

        return new LoginResultDTO
        {
            User = userDto,
            AccessToken = jwt,
            RefreshToken = new RefreshTokenDTO()
            {
                Token = refreshToken,
                ExpireAt = refreshExpiryDate
            },
            AccessTokenExpireAt = expireAt
        };
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

    public async Task<LoginResultDTO> RefreshTokenAsync(string token, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(token);

        if (principal == null)
        {
            throw new SecurityTokenException("Invalid access token");
        }

        var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        // generate new tokens
        var descriptor = GetTokenDescriptor(user);
        var newToken = _jwtSecurityTokenHandler.CreateToken(descriptor);
        var jwt = _jwtSecurityTokenHandler.WriteToken(newToken);

        var newRefreshToken = GenerateRefreshToken();
        var refreshExpiryDate = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = refreshExpiryDate;
        await _dbContext.SaveChangesAsync();

        var userDto = _mapper.Map<UserDTO>(user);

        return new LoginResultDTO
        {
            User = userDto,
            AccessToken = jwt,
            RefreshToken = new RefreshTokenDTO
            {
                Token = newRefreshToken,
                ExpireAt = refreshExpiryDate
            },
            AccessTokenExpireAt = newToken.ValidTo
        };
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtVariables.SecretKey)),
            ValidateLifetime = false
        };

        var principal = _jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
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