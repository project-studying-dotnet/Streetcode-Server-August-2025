using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.DAL.Repositories.Interfaces.Comments;

public interface ICommentRepository : IRepositoryBase<CommentContent>
{
    Task<CommentContent?> GetCommentTreeByCommentIdAsync(int commentId);
}
