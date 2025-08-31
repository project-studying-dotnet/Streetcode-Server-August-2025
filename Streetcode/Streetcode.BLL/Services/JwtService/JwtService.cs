using Streetcode.BLL.Interfaces.Jwt;
using Streetcode.DAL.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Streetcode.DAL.Entities.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Streetcode.BLL.Services.JwtService;

public class JwtService: IJwtService
{
    private readonly JwtEnvironmentVariables _jwtVar;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    private readonly StreetcodeDbContext _dbContext;
    private readonly SigningCredentials _signingCredentials;

    public JwtService(IConfiguration configuration, StreetcodeDbContext dbContext)
    {
        _jwtVar = configuration
            .GetSection("JwtSettings")
            .Get<JwtEnvironmentVariables>()!;

        _dbContext = dbContext;

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtVar.SecretKey));
        _signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
        _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
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
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtVar.ExpiryMinutes),
            SigningCredentials = _signingCredentials,
            Issuer = _jwtVar.Issuer,
            Audience = _jwtVar.Audience
        };
    }
}