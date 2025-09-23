using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Comments;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Comments;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Comments.GetByStreetcodeIdForAdmin
{
    public class GetCommentsByStreetcodeIdForAdminHandler : IRequestHandler<GetCommentsByStreetcodeIdForAdminQuery, Result<IEnumerable<CommentDTO>>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public GetCommentsByStreetcodeIdForAdminHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<CommentDTO>>> Handle(GetCommentsByStreetcodeIdForAdminQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<CommentContent, bool>> predicate = x => x.StreetcodeId == request.StreetcodeId && !x.IsDeleted;

            if (request.IsReviewed.HasValue)
            {
                predicate = x => x.StreetcodeId == request.StreetcodeId && !x.IsDeleted
                              && x.IsReviewed == request.IsReviewed.Value;
            }

            var comments = await _repositoryWrapper.CommentRepository.GetAllAsync(
                predicate,
                include: query => query.Include(c => c.User!));

            if (!comments.Any())
            {
                string errorMsg = Errors_Common.NotFoundByStreetcode.FormatWith("comment", request.StreetcodeId);
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            var ordered = comments.OrderBy(c => c.CreatedAt);
            var commentDtos = _mapper.Map<IEnumerable<CommentDTO>>(ordered);
            return Result.Ok(commentDtos);
        }
    }
}
