using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;

namespace Streetcode.BLL.MediatR.Comments.GetByStreetcodeId;

public record GetCommentsByStreetcodeIdQuery(int StreetcodeId)
    : IRequest<Result<IEnumerable<CommentDTO>>>;
