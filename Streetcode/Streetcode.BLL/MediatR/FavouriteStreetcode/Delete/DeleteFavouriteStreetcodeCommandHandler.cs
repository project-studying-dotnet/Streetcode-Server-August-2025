using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.FavouriteStreetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Realizations.FavouriteStreetcodes;

namespace Streetcode.BLL.MediatR.FavouriteStreetcode.Delete;

public class DeleteFavouriteStreetcodeCommandHandler
    : IRequestHandler<DeleteFavouriteStreetcodeCommand, Result<FavouriteStreetcodeDTO>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly IMapper _mapper;

    public DeleteFavouriteStreetcodeCommandHandler(
        IRepositoryWrapper repositoryWrapper,
        ILoggerService logger,
        IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<FavouriteStreetcodeDTO>> Handle(DeleteFavouriteStreetcodeCommand request, CancellationToken cancellationToken)
    {
        var favourite = await _repositoryWrapper.FavouriteStreetcodeRepository
            .GetFirstOrDefaultAsync(f => f.Id == request.Id);

        if (favourite == null)
        {
            string errorMsg = Errors_Common.NotFoundById.FormatWith("favourite streetcode", request.Id);
            _logger.LogError(request, errorMsg);
            return Result.Fail<FavouriteStreetcodeDTO>(errorMsg);
        }

        bool isAdmin = request.UserRole == UserRole.MainAdministrator
                       || request.UserRole == UserRole.Administrator
                       || request.UserRole == UserRole.Moderator;

        bool isOwner = favourite.UserId == request.RequestingUserId;

        if (!isAdmin && !isOwner)
        {
            string errorMsg = Errors_Common.UnauthorizedAction.FormatWith("delete this favourite streetcode");
            _logger.LogError(request, errorMsg);
            return Result.Fail<FavouriteStreetcodeDTO>(errorMsg);
        }

        _repositoryWrapper.FavouriteStreetcodeRepository.Delete(favourite);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
        if (resultIsSuccess)
        {
            var mappedFavourite = _mapper.Map<FavouriteStreetcodeDTO>(favourite);
            return Result.Ok(mappedFavourite);
        }
        else
        {
            string errorMsg = Errors_Common.FailedToDelete.FormatWith("favourite streetcode");
            _logger.LogError(request, errorMsg);
            return Result.Fail<FavouriteStreetcodeDTO>(new Error(errorMsg));
        }
    }
}