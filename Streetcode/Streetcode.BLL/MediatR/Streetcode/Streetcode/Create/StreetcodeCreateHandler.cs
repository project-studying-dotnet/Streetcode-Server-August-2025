using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.BLL.DTO.Partners;
using Streetcode.DAL.Entities.Partners;
using Streetcode.BLL.DTO.Streetcode.Create;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;

public class StreetcodeCreateHandler : IRequestHandler<StreetcodeCreateCommand, Result<StreetcodeDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public StreetcodeCreateHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<StreetcodeDTO>> Handle(StreetcodeCreateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var streetcodeEntity = _mapper.Map<StreetcodeContent>(request.newStreetcode);

            streetcodeEntity.CreatedAt = DateTime.UtcNow;
            streetcodeEntity.UpdatedAt = DateTime.UtcNow;
            streetcodeEntity.ViewCount = 0;

            _repositoryWrapper.StreetcodeRepository.Create(streetcodeEntity);

            var saveResult = await _repositoryWrapper.SaveChangesAsync();
            if (saveResult == 0)
            {
                const string errorMsg = "Failed to save streetcode to database";
                _logger.LogError(request, errorMsg);
                return Result.Fail<StreetcodeDTO>(new Error(errorMsg));
            }

            var resultDto = _mapper.Map<StreetcodeDTO>(streetcodeEntity);

            _logger.LogInformation($"Success! Streetcode with ID {resultDto.Id} was created");
            return Result.Ok(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(request, ex.Message);
            return Result.Fail<StreetcodeDTO>(ex.Message);
        }
    }
}
