using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Instagram;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Instagram;

namespace Streetcode.BLL.MediatR.Instagram.GetAll;

public class GetAllPostsHandler : IRequestHandler<GetAllPostsQuery, Result<IEnumerable<InstagramPost>>>
{
    private readonly IInstagramService _instagramService;
    private readonly ILoggerService _logger;

    public GetAllPostsHandler(IInstagramService instagramService, ILoggerService logger)
    {
        _instagramService = instagramService;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<InstagramPost>>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all Instagram posts");

        var result = await _instagramService.GetPostsAsync();

        _logger.LogInformation($"Successfully retrieved {result.Count()} Instagram posts");

        return Result.Ok(result);
    }
}