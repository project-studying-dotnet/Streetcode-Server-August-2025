using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Services.Text.Fact;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Create
{
    public class CreateFactHandler : IRequestHandler<CreateFactCommand, Result<FactCreateDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;
        private readonly FactAutoOrder _autoOrder;

        public CreateFactHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, ILoggerService logger)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _autoOrder = new FactAutoOrder(repositoryWrapper);
        }

        public async Task<Result<FactCreateDto>> Handle(CreateFactCommand request, CancellationToken cancellationToken)
        {
            var newFact = _mapper.Map<Facts>(request.newFact);
            if (newFact is null)
            {
                string errorMsg = Errors_Common.CannotConvertNull.FormatWith("fact");
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            if (newFact.ImageId == 0)
            {
                newFact.ImageId = null;
            }

            var streetcodeExists = await _repositoryWrapper.StreetcodeRepository
                .GetFirstOrDefaultAsync(s => s.Id == request.streetcodeId);

            if (streetcodeExists is null)
            {
                string errorMsg = Errors_Streetcode.NotFound;
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            newFact.StreetcodeId = request.streetcodeId;

            if (newFact.Order == 0)
            {
                newFact.Order = await _autoOrder.SetOrderForFacts(request.streetcodeId);
            }

            var entity = await _repositoryWrapper.FactRepository.CreateAsync(newFact);
            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<FactCreateDto>(entity));
            }
            else
            {
                string errorMsg = Errors_Common.FailedToCreate.FormatWith("fact");
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }
    }
}
