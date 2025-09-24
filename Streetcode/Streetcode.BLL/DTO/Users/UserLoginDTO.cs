using System.ComponentModel.DataAnnotations;

namespace Streetcode.BLL.DTO.Users
{
    public class UserLoginDTO
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
