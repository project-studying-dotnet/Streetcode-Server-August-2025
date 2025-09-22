using FluentResults;
using MediatR;
using Streetcode.DAL.Repositories.Interfaces.Base;
using static FluentResults.Result;

namespace Streetcode.BLL.MediatR.Streetcode.Term.Delete
{
    public class DeleteTermHandler : IRequestHandler<DeleteTermCommand, Result<Unit>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public DeleteTermHandler(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<Unit>> Handle(DeleteTermCommand request, CancellationToken cancellationToken)
        {
            var termToDelete = await _repositoryWrapper.TermRepository.GetSingleOrDefaultAsync(t => t.Id == request.Id, null); // Add ', null' here

            if (termToDelete == null)
            {
                return Fail($"Term with id {request.Id} not found.");
            }

            _repositoryWrapper.TermRepository.Delete(termToDelete);
            var isSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (isSuccess)
            {
                return Ok(Unit.Value);
            }
            else
            {
                return Fail("Failed to delete the term.");
            }
        }
    }
}
