namespace Streetcode.BLL.DTO.Users;

public class RegisterUserDTO
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? UserName { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
}