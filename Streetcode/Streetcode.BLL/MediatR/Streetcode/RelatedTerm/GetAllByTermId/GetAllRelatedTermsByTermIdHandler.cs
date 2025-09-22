using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.GetAllByTermId
{
    public record GetAllRelatedTermsByTermIdHandler : IRequestHandler<GetAllRelatedTermsByTermIdQuery, Result<IEnumerable<RelatedTermDTO>>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repository;
        private readonly ILoggerService _logger;

        public GetAllRelatedTermsByTermIdHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, ILoggerService logger)
        {
            _mapper = mapper;
            _repository = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<RelatedTermDTO>>> Handle(GetAllRelatedTermsByTermIdQuery request, CancellationToken cancellationToken)
        {
            var relatedTerms = await _repository.RelatedTermRepository
                .GetAllAsync(
                predicate: rt => rt.TermId == request.id,
                include: rt => rt.Include(rt => rt.Term));

            var relatedTermsDTO = _mapper.Map<IEnumerable<RelatedTermDTO>>(relatedTerms);

            if (relatedTermsDTO is null)
            {
                string errorMsg = Errors_RelatedTerm.FailedToMapDto;
                _logger.LogError(request, errorMsg);
                return new Error(errorMsg);
            }

            return Result.Ok(relatedTermsDTO);
        }
    }
}
