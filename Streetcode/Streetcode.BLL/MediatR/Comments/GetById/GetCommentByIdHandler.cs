using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Comments.GetById
{
    public class GetCommentByIdHandler
     : IRequestHandler<GetCommentByIdQuery, Result<CommentDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public GetCommentByIdHandler(
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CommentDTO>> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
        {
            var comment = await _repositoryWrapper.CommentRepository
             .GetCommentTreeByCommentIdAsync(request.CommentId);

            if (comment == null)
            {
                string errorMsg = Errors_Common.NotFoundById.FormatWith("comment", request.CommentId);
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            if (request.IsReviewed.HasValue)
            {
                comment.Replies = comment.Replies
                    .Where(r => r.IsReviewed == request.IsReviewed.Value)
                    .ToList();
            }

            var dto = _mapper.Map<CommentDTO>(comment);
            return Result.Ok(dto);
        }
    }
}
