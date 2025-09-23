using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;

namespace Streetcode.BLL.MediatR.Comments.SetCommentRestrictedStatus;

public record SetCommentRestrictedStatusCommand(int CommentId, bool IsRestricted) : IRequest<Result<CommentDTO>>;