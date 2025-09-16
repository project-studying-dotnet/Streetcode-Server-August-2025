using Ardalis.Specification.EntityFrameworkCore;
using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.Comments;

namespace Streetcode.DAL.Repositories.Realizations.Comments;

public class CommentRepository : RepositoryBase<CommentContent>, ICommentRepository
{
    public CommentRepository(StreetcodeDbContext dbContext)
        : base(dbContext)
    {
    }
}
