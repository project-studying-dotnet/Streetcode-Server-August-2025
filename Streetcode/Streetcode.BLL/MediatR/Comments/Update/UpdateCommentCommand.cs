using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;

namespace Streetcode.BLL.MediatR.Comments.Update
{
    public record UpdateCommentCommand(CommentUpdateDTO Comment, int RequestingUserId) : IRequest<Result<CommentDTO>>;
}
