using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;

namespace Streetcode.BLL.MediatR.Comments.ReviewComment;

public record ReviewCommentCommand(int Id) : IRequest<Result<CommentDTO>>;