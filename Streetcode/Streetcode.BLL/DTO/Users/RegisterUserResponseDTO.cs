using Streetcode.DAL.Enums;

namespace Streetcode.BLL.DTO.Users;

public class RegisterUserResponseDTO
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
}