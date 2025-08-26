using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;

using Entity = Streetcode.DAL.Entities.Streetcode.TextContent.Term;

namespace Streetcode.BLL.MediatR.Streetcode.Term.Create
{
    public class CreateTermHandler : IRequestHandler<CreateTermCommand, Result<TermDTO>>
    {
        private readonly IRepositoryWrapper _repository;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public CreateTermHandler(IRepositoryWrapper repository, IMapper mapper, ILoggerService logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<TermDTO>> Handle(CreateTermCommand request, CancellationToken cancellationToken)
        {
            var term = _mapper.Map<Entity>(request.Term);

            if (term == null)
            {
                const string errorMsg = "cannot create a new term";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var existingTerms = await _repository.TermRepository.GetAllAsync(predicate: t => t.Id == request.Term.Id && t.Title == request.Term.Title);

            if (existingTerms is null || existingTerms.Any())
            {
                const string errorMsg = "requested term already exists";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var createdTerm = _repository.TermRepository.Create(term);
            var isSuccessResult = await _repository.SaveChangesAsync() > 0;

            if (!isSuccessResult)
            {
                const string errorMsg = "Cannot save changes in database";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var createdTermDTO = _mapper.Map<TermDTO>(createdTerm);

            if(createdTermDTO == null)
            {
                const string errorMsg = "cannot map entity";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            return Result.Ok(createdTermDTO);
        }
    }
}
