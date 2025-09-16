using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Comments.Create;

public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, Result<CommentDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public CreateCommentHandler(
        IRepositoryWrapper repositoryWrapper,
        IMapper mapper,
        ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CommentDTO>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var commentEntity = _mapper.Map<CommentContent>(request.NewComment);
        if (commentEntity is null)
        {
            var errorMsg = Errors_Common.CannotMap.FormatWith("CommentCreateDTO");
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        if (commentEntity.UserId != request.UserId)
        {
            var errorMsg = Errors_Common.UnauthorizedAction.FormatWith("create comment for another user");
            _logger.LogWarning($"User {request.UserId} attempted to create a comment for another user {commentEntity.UserId}");

            return Result.Fail(new Error(errorMsg));
        }

        if (commentEntity.ParentCommentId.HasValue)
        {
            var parent = await _repositoryWrapper.CommentRepository
                .GetFirstOrDefaultAsync(c => c.Id == commentEntity.ParentCommentId.Value);

            if (parent == null)
            {
                var errorMsg = Errors_Common.NotFoundById.FormatWith("parent comment", commentEntity.ParentCommentId.Value);
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }

        commentEntity.CreatedAt = DateTime.UtcNow;

        var createdComment = await _repositoryWrapper.CommentRepository.CreateAsync(commentEntity);
        var isSuccessResult = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (!isSuccessResult)
        {
            var errorMsg = Errors_Common.FailedToCreate.FormatWith("CommentContent");
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var commentDto = _mapper.Map<CommentDTO>(createdComment);
        return Result.Ok(commentDto);
    }
}