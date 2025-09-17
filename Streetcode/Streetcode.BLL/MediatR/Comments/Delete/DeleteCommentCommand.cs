using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.MediatR.Comments.Delete;

public record DeleteCommentCommand(int Id, int RequestingUserId, UserRole UserRole) : IRequest<Result<CommentDTO>>;