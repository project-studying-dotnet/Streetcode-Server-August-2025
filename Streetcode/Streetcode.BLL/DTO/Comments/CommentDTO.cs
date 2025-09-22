using Streetcode.BLL.DTO.Users;

namespace Streetcode.BLL.DTO.Comments;

public class CommentDTO
{
    public int Id { get; set; }
    public string? Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }
    public int StreetcodeId { get; set; }
    public int? ParentCommentId { get; set; }
    public bool IsDeleted { get; set; }

    // If null, the comment was not reviewed yet
    public bool? IsRestricted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public CommentUserDTO? User { get; set; }

    public ICollection<CommentDTO> Replies { get; set; } = new List<CommentDTO>();
    public bool IsReviewed => IsRestricted.HasValue;
}
