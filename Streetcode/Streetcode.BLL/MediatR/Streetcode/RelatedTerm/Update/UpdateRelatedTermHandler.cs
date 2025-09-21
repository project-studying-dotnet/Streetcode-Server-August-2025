using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update
{
    public class UpdateRelatedTermHandler : IRequestHandler<UpdateRelatedTermCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;

        public UpdateRelatedTermHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(UpdateRelatedTermCommand request, CancellationToken cancellationToken)
        {
            var relatedTerm = await _repositoryWrapper.RelatedTermRepository.GetFirstOrDefaultAsync(rt => rt.TermId == request.RelatedTerm.TermId && rt.Word.ToLower() == request.RelatedTerm.Word.ToLower());

            if (relatedTerm is null)
            {
                string errorMsg = Errors_RelatedTerm.NotFoundRelatedTermForTerm.FormatWith(request.RelatedTerm.Word, request.RelatedTerm.TermId);
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _mapper.Map(request.RelatedTerm, relatedTerm);
            _repositoryWrapper.RelatedTermRepository.Update(relatedTerm);

            var isSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (isSuccess)
            {
                return Result.Ok(Unit.Value);
            }
            else
            {
                string errorMsg = Errors_Common.FailedToUpdate.FormatWith("related term");
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }
    }
}