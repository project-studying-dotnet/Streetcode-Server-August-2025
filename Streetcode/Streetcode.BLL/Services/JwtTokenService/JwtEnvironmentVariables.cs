namespace Streetcode.BLL.Services.JwtService;

public class JwtEnvironmentVariables
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpiryMinutes { get; set; }
}