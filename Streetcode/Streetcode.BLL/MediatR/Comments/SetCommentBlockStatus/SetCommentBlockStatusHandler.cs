using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Comments.SetCommentBlockStatus;

public class SetCommentBlockStatusHandler
    : IRequestHandler<SetCommentBlockStatusCommand, Result<CommentDTO>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly IMapper _mapper;

    public SetCommentBlockStatusHandler(
        IRepositoryWrapper repositoryWrapper,
        ILoggerService logger,
        IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<CommentDTO>> Handle(SetCommentBlockStatusCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repositoryWrapper.CommentRepository
            .GetFirstOrDefaultAsync(c => c.Id == request.CommentId);

        if (comment == null)
        {
            string errorMsg = Errors_Common.NotFoundById.FormatWith("comment", request.CommentId);
            _logger.LogError(request, errorMsg);
            return Result.Fail<CommentDTO>(errorMsg);
        }

        comment.IsBlocked = request.Block;
        comment.IsReviewed = true;
        comment.UpdatedAt = DateTime.UtcNow;

        _repositoryWrapper.CommentRepository.Update(comment);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
        if (!resultIsSuccess)
        {
            string errorMsg = Errors_Common.FailedToUpdate.FormatWith("comment");
            _logger.LogError(request, errorMsg);
            return Result.Fail<CommentDTO>(new Error(errorMsg));
        }

        var mappedComment = _mapper.Map<CommentDTO>(comment);
        return Result.Ok(mappedComment);
    }
}