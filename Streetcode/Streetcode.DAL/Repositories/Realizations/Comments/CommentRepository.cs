using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.Comments;
using Streetcode.DAL.Repositories.Realizations.Base;

namespace Streetcode.DAL.Repositories.Realizations.Comments;

public class CommentRepository : Base.RepositoryBase<CommentContent>, ICommentRepository
{
    public CommentRepository(StreetcodeDbContext dbContext)
        : base(dbContext)
    {
    }
}
