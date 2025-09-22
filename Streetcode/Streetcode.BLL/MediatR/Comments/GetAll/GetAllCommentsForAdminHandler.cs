using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Comments.GetAll
{
    public class GetAllCommentsForAdminHandler : IRequestHandler<GetAllCommentsForAdminQuery, Result<IEnumerable<CommentDTO>>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public GetAllCommentsForAdminHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<CommentDTO>>> Handle(GetAllCommentsForAdminQuery request, CancellationToken cancellationToken)
        {
            var comments = await _repositoryWrapper.CommentRepository.GetAllAsync(
                x => !x.IsDeleted && (!request.IsReviewed.HasValue || x.IsReviewed == request.IsReviewed.Value),
                include: query => query.Include(c => c.User!));

            if (comments is null)
            {
                string errorMsg = Errors_Common.NotFoundAny.FormatWith("comments");
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var dtos = _mapper.Map<IEnumerable<CommentDTO>>(comments);
            return Result.Ok(dtos);
        }
    }
}
