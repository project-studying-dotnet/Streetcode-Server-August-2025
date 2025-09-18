using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Interfaces.Redis;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.Delete;

public class DeleteStreetcodeHandler : IRequestHandler<DeleteStreetcodeCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly IRedisService<StreetcodeContent> _redisService;

    public DeleteStreetcodeHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger, IRedisService<StreetcodeContent> redisService)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
        _redisService = redisService;
    }

    public async Task<Result<Unit>> Handle(DeleteStreetcodeCommand request, CancellationToken cancellationToken)
    {
        var streetcode = await _repositoryWrapper.StreetcodeRepository
            .GetFirstOrDefaultAsync(s => s.Id == request.Id);

        if (streetcode is null)
        {
            string errorMsg = Errors_Common.NotFoundById.FormatWith("Streetcode", request.Id);
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        _repositoryWrapper.StreetcodeRepository.Delete(streetcode);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (resultIsSuccess)
        {
            var redisKeys = new[]
                {
                    $"streetcodeById:{streetcode.Id}",
                    $"streetcodeByIndex:{streetcode.Index}",
                    $"streetcodeByUrl:{streetcode.TransliterationUrl}",
                    $"streetcodeShortById:{streetcode.Id}"
                };

            await _redisService.DeleteAsync(redisKeys, cancellationToken);
            return Result.Ok(Unit.Value);
        }

        string failMsg = Errors_Common.FailedToDelete.FormatWith("Streetcode");
        _logger.LogError(request, failMsg);
        return Result.Fail(new Error(failMsg));
    }
}
