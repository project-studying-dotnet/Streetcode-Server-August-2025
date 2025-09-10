using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Delete
{
    public class DeleteFactHandler : IRequestHandler<DeleteFactCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public DeleteFactHandler(IRepositoryWrapper repositoryWrapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(DeleteFactCommand request, CancellationToken cancellationToken)
        {
            int id = request.id;
            var fact = await _repositoryWrapper.FactRepository.GetFirstOrDefaultAsync(f => f.Id == id);
            if (fact == null)
            {
                string errorMsg = Errors_Common.NotFoundById.FormatWith("fact", id);
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            if (fact.Image is not null)
            {
                _repositoryWrapper.ImageRepository.Delete(fact.Image);
            }

            _repositoryWrapper.FactRepository.Delete(fact);
            var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
            if (resultIsSuccess)
            {
                return Result.Ok(Unit.Value);
            }
            else
            {
                string errorMsg = Errors_Common.FailedToDelete.FormatWith("fact");
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }
    }
}
