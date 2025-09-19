using Streetcode.DAL.Enums;

namespace Streetcode.BLL.DTO.Users;

public class CommentUserDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public UserRole Role { get; set; }
    public string? UserName { get; set; }

    // Computed property for display name
    public string DisplayName => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Surname)
        ? $"{Name} {Surname}".Trim()
        : "Anonymous User";

    public string Initials => GetInitials(Name, Surname);

    private string GetInitials(string? firstName, string? lastName)
    {
        var initials = "";
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            initials += firstName[0].ToString().ToUpper();
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            initials += lastName[0].ToString().ToUpper();
        }

        return string.IsNullOrEmpty(initials) ? "AU" : initials;
    }
}