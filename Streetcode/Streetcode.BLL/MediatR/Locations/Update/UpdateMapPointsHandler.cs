using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Locations.Update;

public class UpdateMapPointsHandler : IRequestHandler<UpdateMapPointCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public UpdateMapPointsHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(UpdateMapPointCommand request, CancellationToken cancellationToken)
    {
        var mapPoint = await _repositoryWrapper.StatisticRecordRepository.GetFirstOrDefaultAsync(x => x.Id == request.Id);

        if (mapPoint is null)
        {
            string errorMsg = Errors_Common.NotFoundById.FormatWith("MapPoint", request.Id);
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        mapPoint.Count++;
        _repositoryWrapper.StatisticRecordRepository.Update(mapPoint);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync();

        if (resultIsSuccess >= 0)
        {
            string errorMsg = Errors_Common.CannotSaveTheData;
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        return Result.Ok(Unit.Value);
    }
}
