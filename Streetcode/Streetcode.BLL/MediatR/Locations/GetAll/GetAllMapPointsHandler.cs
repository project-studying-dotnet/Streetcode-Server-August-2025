using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Locations;
using Streetcode.BLL.Interfaces.Logging;
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

    public Task<Result<IEnumerable<MapPointDTO>>> Handle(GetAllMapPointsQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
