using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.DTO.Users
{
    public class LoginResultDTO
    {
        public UserDTO User { get; set; }
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpireAt { get; set; }
        public RefreshTokenDTO RefreshToken { get; set; }
    }
}
