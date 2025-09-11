using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.DTO.Users
{
    public class TokenDTO
    {
        public string Token { get; set; }
        public DateTime ExpireAt { get; set; }
    }
}
