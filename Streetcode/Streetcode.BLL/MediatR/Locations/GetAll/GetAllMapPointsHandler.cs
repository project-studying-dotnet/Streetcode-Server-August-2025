using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Locations;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Locations.GetAll;

public class GetAllMapPointsHandler : IRequestHandler<GetAllMapPointsQuery, Result<IEnumerable<MapPointDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetAllMapPointsHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<MapPointDTO>>> Handle(GetAllMapPointsQuery request, CancellationToken cancellationToken)
    {
        var mapPoints = await _repositoryWrapper.StatisticRecordRepository.GetAllAsync(include: x => x.Include(x => x.StreetcodeCoordinate));

        if (mapPoints is null)
        {
            string errorMsg = Errors_Common.NotFoundAny.FormatWith("map points");
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var mappedEntities = _mapper.Map<IEnumerable<MapPointDTO>>(mapPoints);

        if (mappedEntities is null)
        {
            string errorMsg = Errors_Common.CannotMap.FormatWith("map points");
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        return Result.Ok(mappedEntities.OrderByDescending(x => x.PlateNumber).AsEnumerable());
    }
}
