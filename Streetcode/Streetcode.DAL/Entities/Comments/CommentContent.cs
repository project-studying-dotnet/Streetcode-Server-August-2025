using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Users;

namespace Streetcode.DAL.Entities.Comments;

[Table("comments", Schema = "comment")]
public class CommentContent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string? Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int StreetcodeId { get; set; }
    public StreetcodeContent? Streetcode { get; set; }

    public int? ParentCommentId { get; set; }
    public CommentContent? ParentComment { get; set; }
    public bool IsDeleted { get; set; }

    // If null, the comment was not reviewed yet
    public bool? IsRestricted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<CommentContent> Replies { get; set; } = new List<CommentContent>();

    [NotMapped]
    public bool IsReviewed => IsRestricted.HasValue;
}
