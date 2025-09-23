using System.Security.Claims;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Favourite;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.CreateFavourite;

public class CreateFavouriteStreetcodeHandler : IRequestHandler<CreateFavouriteStreetcodeCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly HttpContextAccessor _httpContextAccessor;

    public CreateFavouriteStreetcodeHandler(
        IRepositoryWrapper repositoryWrapper,
        ILoggerService logger,
        HttpContextAccessor httpContextAccessor)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<Unit>> Handle(CreateFavouriteStreetcodeCommand request, CancellationToken cancellationToken)
    {
        var userIdString = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            var errorMsg = Errors_Jwt.UserNotFound;
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        var userId = int.Parse(userIdString);

        if(await _repositoryWrapper.FavoriteStreetcodeRepository.GetFirstOrDefaultAsync(
               f => f.StreetcodeId == request.StreetcodeId && f.UserId == userId) is not null)
        {
            var errorMsg = Errors_Common.AlreadyExists.FormatWith("Favourite streetcode");
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        var favouriteStreetcode = new FavouriteStreetcode
        {
            StreetcodeId = request.StreetcodeId,
            UserId = userId
        };

        await _repositoryWrapper.FavoriteStreetcodeRepository.CreateAsync(favouriteStreetcode);
        var resultSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (!resultSuccess)
        {
            var errorMsg = Errors_Common.FailedToCreate.FormatWith("favourite streetcode");
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        return Result.Ok(Unit.Value);
    }
}
