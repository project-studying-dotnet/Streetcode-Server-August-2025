using Microsoft.EntityFrameworkCore;
using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.Comments;
using Streetcode.DAL.Repositories.Realizations.Base;

namespace Streetcode.DAL.Repositories.Realizations.Comments;

public class CommentRepository : RepositoryBase<CommentContent>, ICommentRepository
{
    public CommentRepository(StreetcodeDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<CommentContent?> GetCommentTreeByCommentIdAsync(int commentId)
    {
        var rootExists = await GetFirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

        if (rootExists == null)
        {
            return null;
        }

        var streetcodeId = rootExists.StreetcodeId;

        var allComments = await GetAllAsync(
            c => c.StreetcodeId == streetcodeId && !c.IsDeleted,
            include: query => query.Include(c => c.User)););

        var commentList = allComments.ToList();

        var commentDict = commentList.ToDictionary(c => c.Id, c => c);

        foreach (var comment in commentList)
        {
            if (comment.ParentCommentId.HasValue && commentDict.ContainsKey(comment.ParentCommentId.Value))
            {
                commentDict[comment.ParentCommentId.Value].Replies.Add(comment);
            }
        }

        return commentDict.ContainsKey(commentId) ? commentDict[commentId] : null;
    }

    public async Task<IEnumerable<CommentContent>> GetCommentTreeByStreetcodeIdAsync(int streetcodeId)
    {
        var allComments = await GetAllAsync(
            predicate: c => c.StreetcodeId == streetcodeId && !c.IsDeleted,
            include: query => query.Include(c => c.User));

        // Convert to list for dictionary operations
        var commentList = allComments.ToList();

        // Build the tree structure
        var commentDict = commentList.ToDictionary(c => c.Id, c => c);
        var rootComments = new List<CommentContent>();

        foreach (var comment in commentList)
        {
            if (comment.ParentCommentId.HasValue && commentDict.ContainsKey(comment.ParentCommentId.Value))
            {
                var parent = commentDict[comment.ParentCommentId.Value];
                parent.Replies.Add(comment);
            }
            else
            {
                rootComments.Add(comment);
            }
        }

        // Sort root comments by creation date
        return rootComments.OrderBy(c => c.CreatedAt);
    }
}
