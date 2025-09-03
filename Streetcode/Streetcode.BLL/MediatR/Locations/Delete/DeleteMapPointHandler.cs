using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Locations.Delete;

public class DeleteMapPointHandler : IRequestHandler<DeleteMapPointCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public DeleteMapPointHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteMapPointCommand request, CancellationToken cancellationToken)
    {
        var mapPoint = await _repositoryWrapper.StatisticRecordRepository.GetFirstOrDefaultAsync(x => x.Id == request.Id);

        if (mapPoint is null)
        {
            string errorMsg = "CannotFindPointWithId";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        _repositoryWrapper.StatisticRecordRepository.Delete(mapPoint);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync();

        if (resultIsSuccess >= 0)
        {
            string errorMsg = "FailedToDeleteThePoint";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        return Result.Ok(Unit.Value);
    }
}
