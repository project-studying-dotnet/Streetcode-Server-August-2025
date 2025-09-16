using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.Delete;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Comments.Delete;

public class DeleteCommentCommandHandler
    : IRequestHandler<DeleteCommentCommand, Result<CommentDTO>>
{

    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly IMapper _mapper;
    public DeleteCommentCommandHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger, IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<CommentDTO>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        int commendId = request.Id;
        var comment = await _repositoryWrapper.CommentRepository.GetFirstOrDefaultAsync(c => c.Id == commendId);
        if (comment == null)
        {
            string errorMsg = Errors_Common.NotFoundById.FormatWith("comment", commendId);
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        bool isAdmin = request.UserRole == UserRole.MainAdministrator
                       || request.UserRole == UserRole.Administrator
                       || request.UserRole == UserRole.Moderator;

        bool isOwner = comment.UserId == request.RequestingUserId;

        if (!isAdmin && !isOwner)
        {
            string errorMsg = Errors_Common.UnauthorizedAction.FormatWith("delete this comment");
            _logger.LogError(request, errorMsg);
            return Result.Fail<CommentDTO>(errorMsg);
        }

        _repositoryWrapper.CommentRepository.Delete(comment);
        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
        if (resultIsSuccess)
        {
            var mappedComment = _mapper.Map<CommentDTO>(comment);
            return Result.Ok(mappedComment);
        }
        else
        {
            string errorMsg = Errors_Common.FailedToDelete.FormatWith("comment");
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}