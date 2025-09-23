using Streetcode.BLL.DTO.Users.GoogleLogin;

namespace Streetcode.BLL.Interfaces.Google;
public interface IGoogleService
{
    Task<GoogleUserInfoDTO> GetGoogleUserInfoAsync();
}
