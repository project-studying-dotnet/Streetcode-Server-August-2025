using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;

namespace Streetcode.BLL.MediatR.Comments.SetCommentBlockStatus;

public record SetCommentBlockStatusCommand(int CommentId, bool Block) : IRequest<Result<CommentDTO>>;