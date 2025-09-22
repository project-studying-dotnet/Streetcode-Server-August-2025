using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Comments.SetCommentRestrictedStatus;

public class SetCommentRestrictedStatusHandler
    : IRequestHandler<SetCommentRestrictedStatusCommand, Result<CommentDTO>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly IMapper _mapper;

    public SetCommentRestrictedStatusHandler(
        IRepositoryWrapper repositoryWrapper,
        ILoggerService logger,
        IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<CommentDTO>> Handle(SetCommentRestrictedStatusCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repositoryWrapper.CommentRepository
            .GetFirstOrDefaultAsync(c => c.Id == request.CommentId);

        if (comment == null)
        {
            string errorMsg = Errors_Common.NotFoundById.FormatWith("comment", request.CommentId);
            _logger.LogError(request, errorMsg);
            return Result.Fail<CommentDTO>(errorMsg);
        }

        if (comment.IsRestricted == request.IsRestricted && comment.IsReviewed)
        {
            return Result.Ok(_mapper.Map<CommentDTO>(comment));
        }

        comment.IsRestricted = request.IsRestricted;
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