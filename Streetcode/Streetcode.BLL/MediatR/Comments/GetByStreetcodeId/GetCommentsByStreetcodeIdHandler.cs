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

namespace Streetcode.BLL.MediatR.Comments.GetByStreetcodeId;

public class GetCommentsByStreetcodeIdHandler : IRequestHandler<GetCommentsByStreetcodeIdQuery, Result<IEnumerable<CommentDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetCommentsByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<CommentDTO>>> Handle(GetCommentsByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var commentTree = await _repositoryWrapper.CommentRepository.GetCommentTreeByStreetcodeIdAsync(request.StreetcodeId);

        if (commentTree is null)
        {
            string errorMsg = Errors_Common.NotFoundByStreetcode.FormatWith("comment", request.StreetcodeId);
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var commentDtos = _mapper.Map<IEnumerable<CommentDTO>>(commentTree);
        return Result.Ok(commentDtos);
    }
}
