using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;

namespace Streetcode.BLL.MediatR.Comments.Create;

public record CreateCommentCommand(CommentCreateDTO NewComment)
    : IRequest<Result<CommentDTO>>;