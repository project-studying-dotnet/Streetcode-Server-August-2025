using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Redis;
using Streetcode.BLL.Services.Redis;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetShortById
{
    public class GetStreetcodeShortByIdHandler : IRequestHandler<GetStreetcodeShortByIdQuery, Result<StreetcodeShortDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repository;
        private readonly ILoggerService _logger;
        private readonly IRedisService<StreetcodeShortDTO> _redisService;

        public GetStreetcodeShortByIdHandler(IMapper mapper, IRepositoryWrapper repository, ILoggerService logger, IRedisService<StreetcodeShortDTO> redisService)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
            _redisService = redisService;
        }

        public async Task<Result<StreetcodeShortDTO>> Handle(GetStreetcodeShortByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"streetcodeShortById:{request.id}";

            var streetcodeShortDtoFromCache = await _redisService.GetAsync(cacheKey, cancellationToken);
            if (streetcodeShortDtoFromCache is not null)
            {
                return Result.Ok(streetcodeShortDtoFromCache);
            }

            var streetcode = await _repository.StreetcodeRepository.GetFirstOrDefaultAsync(st => st.Id == request.id);

            if (streetcode == null)
            {
                string errorMsg = Errors_Common.NotFoundById.FormatWith("streetcode", request.id);
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            var streetcodeShortDto = _mapper.Map<StreetcodeShortDTO>(streetcode);

            if(streetcodeShortDto == null)
            {
                string errorMsg = Errors_Streetcode.CannotMap.FormatWith("shortDTO");
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            await _redisService.SetAsync(cacheKey, streetcodeShortDto, TimeToLiveOption.Minutes, 30, cancellationToken);

            return Result.Ok(streetcodeShortDto);
        }
    }
}
