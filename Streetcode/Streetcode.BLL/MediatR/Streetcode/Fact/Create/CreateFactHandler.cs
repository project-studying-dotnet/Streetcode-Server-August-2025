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

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Create
{
    public class CreateFactHandler : IRequestHandler<CreateFactCommand, Result<FactDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public CreateFactHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, ILoggerService logger)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<FactDto>> Handle(CreateFactCommand request, CancellationToken cancellationToken)
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

            var entity = _repositoryWrapper.FactRepository.Create(newFact);
            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<FactDto>(entity));
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
