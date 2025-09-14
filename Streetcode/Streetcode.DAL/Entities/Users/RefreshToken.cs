using System.ComponentModel.DataAnnotations.Schema;

namespace Streetcode.DAL.Entities.Users;

[Table("RefreshTokens", Schema = "RefreshTokens")]
public class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public string Token { get; set; }
    public bool IsRevoked { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}