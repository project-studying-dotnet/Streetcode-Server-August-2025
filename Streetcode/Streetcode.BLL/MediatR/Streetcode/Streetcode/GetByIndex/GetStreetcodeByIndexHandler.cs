using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Redis;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetByIndex;

public class GetStreetcodeByIndexHandler : IRequestHandler<GetStreetcodeByIndexQuery, Result<StreetcodeDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly IRedisService<StreetcodeDTO> _redisService;

    public GetStreetcodeByIndexHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger, IRedisService<StreetcodeDTO> redisService)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
        _redisService = redisService;
    }

    public async Task<Result<StreetcodeDTO>> Handle(GetStreetcodeByIndexQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"streetcodeByIndex:{request.Index}";

        var streetcodeFromCache = await _redisService.GetAsync(cacheKey, cancellationToken);

        if (streetcodeFromCache is not null)
        {
            return Result.Ok(streetcodeFromCache);
        }

        var streetcode = await _repositoryWrapper.StreetcodeRepository.GetFirstOrDefaultAsync(
            predicate: st => st.Index == request.Index,
            include: source => source.Include(l => l.Tags));

        if (streetcode is null)
        {
            string errorMsg = Errors_Streetcode.NotFoundBy.FormatWith("index", request.Index);
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var streetcodeDto = _mapper.Map<StreetcodeDTO>(streetcode);

        await _redisService.SetAsync(cacheKey, streetcodeDto, Services.Redis.TimeToLiveOption.Minutes, 30, cancellationToken);

        return Result.Ok(streetcodeDto);
    }
}