using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;

namespace Streetcode.BLL.MediatR.Comments.Delete;

public record DeleteCommentCommand(int id) : IRequest<Result<CommentDTO>>;