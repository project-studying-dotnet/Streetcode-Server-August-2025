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
        var newStreetcode = _mapper.Map<StreetcodeContent>(request.newStreetcode);
        try
        {
            newStreetcode = await _repositoryWrapper.StreetcodeRepository.CreateAsync(newStreetcode);
            await _repositoryWrapper.SaveChangesAsync();

            var resultDto = _mapper.Map<StreetcodeDTO>(newStreetcode);

            return Result.Ok(resultDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(request, ex.Message);
            return Result.Fail(ex.Message);
        }
    }
}
