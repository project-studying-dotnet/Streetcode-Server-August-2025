using FluentResults;
using MediatR;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.ArtGallery.GetSlidesCountByStreetcodeId;

public class GetSlidesCountByStreetcodeIdHandler : IRequestHandler<GetSlidesCountByStreetcodeIdQuery, Result<int>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;

    public GetSlidesCountByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;
    }

    public Task<Result<int>> Handle(GetSlidesCountByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var query = _repositoryWrapper.StreetcodeArtSlideRepository
            .FindAll(predicate: s => s.StreetcodeId == request.StreetcodeId);

        if (query is null)
        {
            return Task.FromResult(Result.Ok(0));
        }

        var count = query.Count();
        return Task.FromResult(Result.Ok(count));
    }
}
