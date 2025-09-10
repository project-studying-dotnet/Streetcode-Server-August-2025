using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Services.Text.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Create
{
    public class CreateFactHandler : IRequestHandler<CreateFactCommand, Result<FactCreateDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;
        private readonly FactAutoOrder _autoOrder;

        public CreateFactHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, ILoggerService logger, FactAutoOrder autoOrder)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
            _autoOrder = autoOrder;
        }

        public async Task<Result<FactCreateDto>> Handle(CreateFactCommand request, CancellationToken cancellationToken)
        {
            var newFact = _mapper.Map<Facts>(request.newFact);
            if (newFact is null)
            {
                const string errorMsg = "Cannot convert null to fact";
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
                const string errorMsg = "Streetcode not found";
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
                const string errorMsg = "Failed to create a fact";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }
    }
}
