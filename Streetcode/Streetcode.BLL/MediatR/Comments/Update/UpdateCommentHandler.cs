using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Comments.Update
{
    public class UpdateCommentHandler : IRequestHandler<UpdateCommentCommand, Result<CommentDTO>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public UpdateCommentHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CommentDTO>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var comment = await _repositoryWrapper.CommentRepository.GetFirstOrDefaultAsync(c => c.Id == request.Comment.Id);

                if (comment is null)
                {
                    string errorMsg = Errors_Common.NotFoundById.FormatWith("comment", request.Comment.Id);
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(errorMsg);
                }

                bool isOwner = comment.UserId == request.RequestingUserId;

                if (!isOwner)
                {
                    string errorMsg = Errors_Common.UnauthorizedAction.FormatWith("update this comment");
                    _logger.LogError(request, errorMsg);
                    return Result.Fail<CommentDTO>(errorMsg);
                }

                _mapper.Map(request.Comment, comment);
                comment.UpdatedAt = DateTime.UtcNow;

                _repositoryWrapper.CommentRepository.Update(comment);

                var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
                if (resultIsSuccess)
                {
                    return Result.Ok(_mapper.Map<CommentDTO>(comment));
                }
                else
                {
                    string errorMsg = Errors_Common.FailedToUpdate.FormatWith("comment");
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(errorMsg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(request, ex.Message);
                return Result.Fail(ex.Message);
            }
        }
    }
}
