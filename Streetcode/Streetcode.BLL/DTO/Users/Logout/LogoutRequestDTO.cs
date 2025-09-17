using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.DTO.Users.Logout
{
    public class LogoutRequestDTO
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
