using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Comments.ReviewComment;

public class ReviewCommentCommandHandler
    : IRequestHandler<ReviewCommentCommand, Result<CommentDTO>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly IMapper _mapper;

    public ReviewCommentCommandHandler(
        IRepositoryWrapper repositoryWrapper,
        ILoggerService logger,
        IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<CommentDTO>> Handle(ReviewCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repositoryWrapper.CommentRepository
            .GetFirstOrDefaultAsync(c => c.Id == request.Id);

        if (comment == null)
        {
            string errorMsg = Errors_Common.NotFoundById.FormatWith("comment", request.Id);
            _logger.LogError(request, errorMsg);
            return Result.Fail<CommentDTO>(errorMsg);
        }

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