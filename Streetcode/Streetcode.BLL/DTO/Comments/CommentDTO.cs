using Streetcode.DAL.Entities.Comments;

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
    public bool IsReviewed { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<CommentDTO> Replies { get; set; } = new List<CommentDTO>();
}
