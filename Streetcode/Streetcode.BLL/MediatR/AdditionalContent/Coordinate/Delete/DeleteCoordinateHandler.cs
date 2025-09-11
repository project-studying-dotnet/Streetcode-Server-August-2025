using FluentResults;
using MediatR;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.AdditionalContent.Coordinate.Delete;

public class DeleteCoordinateHandler : IRequestHandler<DeleteCoordinateCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public DeleteCoordinateHandler(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<Result<Unit>> Handle(DeleteCoordinateCommand request, CancellationToken cancellationToken)
    {
        var streetcodeCoordinate = await _repositoryWrapper.StreetcodeCoordinateRepository.GetFirstOrDefaultAsync(f => f.Id == request.Id);

        if (streetcodeCoordinate is null)
        {
            return Result.Fail(new Error(Errors_AdditionalContent.Coordinate_NotFoundByCategory.FormatWith(request.Id)));
        }

        _repositoryWrapper.StreetcodeCoordinateRepository.Delete(streetcodeCoordinate);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
        return resultIsSuccess ? Result.Ok(Unit.Value) : Result.Fail(new Error(Errors_Common.FailedToDelete.FormatWith("streetcodeCoordinate")));
    }
}