using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;

namespace Streetcode.BLL.MediatR.Comments.GetByStreetcodeIdForAdmin
{
    public record GetCommentsByStreetcodeIdForAdminQuery(int StreetcodeId, bool? IsReviewed = null)
     : IRequest<Result<IEnumerable<CommentDTO>>>;
}
