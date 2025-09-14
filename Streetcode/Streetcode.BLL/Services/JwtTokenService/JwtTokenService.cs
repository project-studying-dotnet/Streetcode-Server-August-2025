using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using FluentResults;
using Streetcode.BLL.DTO.Users;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.Services.JwtService;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtEnvironmentVariables _jwtVariables;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly SigningCredentials _signingCredentials;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;

    public JwtTokenService(IConfiguration configuration, IMapper mapper, IRepositoryWrapper repositoryWrapper)
    {
        _jwtVariables = configuration
            .GetSection("JwtSettings")
            .Get<JwtEnvironmentVariables>()!;

        _mapper = mapper;
        _repositoryWrapper = repositoryWrapper;

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtVariables.SecretKey));
        _signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    }

    public async Task<Result<LoginResultDTO>> GenerateTokenAsync(int userId)
    {
        var user = await _repositoryWrapper.UserRepository.GetFirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return Result.Fail<LoginResultDTO>("User with this userId was not found");
        }

        try
        {
            // Create JWT access token
            var descriptor = GetTokenDescriptor(user);
            var token = _jwtSecurityTokenHandler.CreateToken(descriptor);
            var jwt = _jwtSecurityTokenHandler.WriteToken(token);

            var expireAt = token.ValidTo;

            // Create refresh token
            var refreshTokenString = GenerateRefreshToken();
            var refreshExpiryDate = DateTime.UtcNow.AddDays(7);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenString,
                ExpiresAt = refreshExpiryDate,
                IsRevoked = false
            };

            await _repositoryWrapper.RefreshTokenRepository.CreateAsync(refreshToken);
            await _repositoryWrapper.SaveChangesAsync();
            var userDto = _mapper.Map<UserDTO>(user);

            return Result.Ok(new LoginResultDTO
            {
                User = userDto,
                AccessToken = new TokenDTO
                {
                    Token = jwt,
                    ExpireAt = expireAt
                },
                RefreshToken = new TokenDTO
                {
                    Token = refreshTokenString,
                    ExpireAt = refreshExpiryDate
                },
            });
        }
        catch (Exception ex)
        {
            return Result.Fail<LoginResultDTO>(new ExceptionalError(ex));
        }
    }

    public Result<ClaimsPrincipal> ValidateToken(string token)
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
        catch (SecurityTokenExpiredException)
        {
            return Result.Fail<ClaimsPrincipal>("Token has expired");
        }
        catch (SecurityTokenValidationException)
        {
            return Result.Fail<ClaimsPrincipal>("Token validation failed");
        }
        catch (Exception ex)
        {
            return Result.Fail<ClaimsPrincipal>(new ExceptionalError(ex));
        }
    }

    public Result<int> GetUserIdFromToken(string token)
    {
        var validationResult = ValidateToken(token);
        if (validationResult.IsFailed)
        {
            return Result.Fail<int>(validationResult.Errors);
        }

        var principal = validationResult.Value;

        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
        {
            return Result.Fail<int>("UserId claim not found in token");
        }

        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return Result.Fail<int>($"UserId claim has invalid format: {userIdClaim.Value}");
        }

        return Result.Ok(userId);
    }

    public async Task<Result<LoginResultDTO>> RefreshTokenAsync(string token, string refreshToken)
    {
        // Validate expired access token to get userId
        var userResult = await GetUserFromExpiredTokenAsync(token);
        if (userResult.IsFailed)
        {
            return Result.Fail<LoginResultDTO>(userResult.Errors);
        }

        var user = userResult.Value;

        // Check the refresh token in DB
        var refreshTokenResult = await ValidateRefreshTokenAsync(user.Id, refreshToken);
        if (refreshTokenResult.IsFailed)
        {
            return Result.Fail<LoginResultDTO>(refreshTokenResult.Errors);
        }

        var storedRefreshToken = refreshTokenResult.Value;

        // Generate new access token
        var descriptor = GetTokenDescriptor(user);
        var newToken = _jwtSecurityTokenHandler.CreateToken(descriptor);
        var jwt = _jwtSecurityTokenHandler.WriteToken(newToken);

        // Generate new refresh token
        var newRefreshTokenString = GenerateRefreshToken();
        var newRefreshExpiryDate = DateTime.UtcNow.AddDays(7);
        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenString,
            ExpiresAt = newRefreshExpiryDate,
            IsRevoked = false
        };

        // Revoke the old token and save the new one
        storedRefreshToken.IsRevoked = true;
        _repositoryWrapper.RefreshTokenRepository.Update(storedRefreshToken);
        await _repositoryWrapper.RefreshTokenRepository.CreateAsync(newRefreshToken);
        await _repositoryWrapper.SaveChangesAsync();

        var userDto = _mapper.Map<UserDTO>(user);

        var result = new LoginResultDTO
        {
            User = userDto,
            AccessToken = new TokenDTO
            {
                Token = jwt,
                ExpireAt = newToken.ValidTo
            },
            RefreshToken = new TokenDTO
            {
                Token = newRefreshTokenString,
                ExpireAt = newRefreshExpiryDate
            },
        };

        return Result.Ok(result);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<Result> RevokeRefreshTokenAsync(string refreshToken)
    {
        try
        {
            var storedToken = await _repositoryWrapper.RefreshTokenRepository
                .GetFirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken is null)
            {
                return Result.Fail("Refresh token not found");
            }

            if (storedToken.IsRevoked)
            {
                return Result.Fail("Refresh token is already revoked");
            }

            storedToken.IsRevoked = true;
            _repositoryWrapper.RefreshTokenRepository.Update(storedToken);

            await _repositoryWrapper.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex));
        }
    }

    private async Task<Result<User>> GetUserFromExpiredTokenAsync(string token)
    {
        // 1. Validate the expired access token
        var principalResult = GetPrincipalFromExpiredToken(token);
        if (principalResult.IsFailed)
        {
            return Result.Fail<User>(principalResult.Errors);
        }

        var principal = principalResult.Value;

        // 2. Extract userId claim
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Result.Fail<User>("UserId claim missing or invalid");
        }

        // 3. Load user from DB
        var user = await _repositoryWrapper.UserRepository
            .GetFirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            return Result.Fail<User>("User not found");
        }

        return Result.Ok(user);
    }

    private async Task<Result<RefreshToken>> ValidateRefreshTokenAsync(int userId, string refreshToken)
    {
        var storedRefreshToken = await _repositoryWrapper.RefreshTokenRepository
            .GetFirstOrDefaultAsync(rt => rt.UserId == userId && rt.Token == refreshToken);

        if (storedRefreshToken is null)
        {
            return Result.Fail<RefreshToken>("Refresh token not found for this user");
        }

        if (storedRefreshToken.IsRevoked)
        {
            return Result.Fail<RefreshToken>("Refresh token has been revoked");
        }

        if (storedRefreshToken.IsExpired)
        {
            return Result.Fail<RefreshToken>("Refresh token has expired");
        }

        return Result.Ok(storedRefreshToken);
    }

    private Result<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtVariables.SecretKey)),
            ValidateLifetime = false
        };

        try
        {
            var principal = _jwtSecurityTokenHandler.ValidateToken(
                token,
                tokenValidationParameters,
                out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return Result.Fail<ClaimsPrincipal>("Invalid token: unsupported or mismatched algorithm");
            }

            return Result.Ok(principal);
        }
        catch(SecurityTokenException ex)
        {
            return Result.Fail<ClaimsPrincipal>(new Error("Token validation failed").CausedBy(ex));
        }
        catch (Exception ex)
        {
            return Result.Fail<ClaimsPrincipal>(new ExceptionalError(ex));
        }
    }

    private SecurityTokenDescriptor GetTokenDescriptor(User user)
    {
        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.GivenName, user.Name),
                new Claim(ClaimTypes.Name, user.UserName),
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