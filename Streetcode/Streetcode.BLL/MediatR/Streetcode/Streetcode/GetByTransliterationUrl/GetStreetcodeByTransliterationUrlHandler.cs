using Ardalis.Specification;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Redis;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetByTransliterationUrl
{
  public class GetStreetcodeByTransliterationUrlHandler : IRequestHandler<GetStreetcodeByTransliterationUrlQuery, Result<StreetcodeDTO>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;
        private readonly IRedisService<StreetcodeDTO> _redisService;

        public GetStreetcodeByTransliterationUrlHandler(IRepositoryWrapper repository, IMapper mapper, ILoggerService logger, IRedisService<StreetcodeDTO> redisService)
        {
            _repositoryWrapper = repository;
            _mapper = mapper;
            _logger = logger;
            _redisService = redisService;
        }

        public async Task<Result<StreetcodeDTO>> Handle(GetStreetcodeByTransliterationUrlQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"streetcodeByUrl:{request.url}";

            var streetcodeDtoFromCache = await _redisService.GetAsync(cacheKey, cancellationToken);
            if (streetcodeDtoFromCache is not null)
            {
                return Result.Ok(streetcodeDtoFromCache);
            }

            var streetcode = await _repositoryWrapper.StreetcodeRepository
                .GetFirstOrDefaultAsync(
                    predicate: st => st.TransliterationUrl == request.url);

            if (streetcode == null)
            {
                string errorMsg = $"Cannot find streetcode by transliteration url: {request.url}";
                _logger.LogError(request, errorMsg);
                return new Error(errorMsg);
            }

            var tagIndexed = await _repositoryWrapper.StreetcodeTagIndexRepository
                                    .GetAllAsync(
                                        t => t.StreetcodeId == streetcode.Id,
                                        include: q => q.Include(ti => ti.Tag));

            var streetcodeDto = _mapper.Map<StreetcodeDTO>(streetcode);
            streetcodeDto.Tags = _mapper.Map<List<StreetcodeTagDTO>>(tagIndexed);

            await _redisService.SetAsync(cacheKey, streetcodeDto, Services.Redis.TimeToLiveOption.Minutes, 30, cancellationToken);

            return Result.Ok(streetcodeDto);
        }
    }
}
