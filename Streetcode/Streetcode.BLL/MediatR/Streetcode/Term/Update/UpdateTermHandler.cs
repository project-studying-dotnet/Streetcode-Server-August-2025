using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using static FluentResults.Result;

namespace Streetcode.BLL.MediatR.Streetcode.Term.Update;

public class UpdateTermHandler : IRequestHandler<UpdateTermCommand, Result<TermDTO>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;

    public UpdateTermHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
    }

    public async Task<Result<TermDTO>> Handle(UpdateTermCommand request, CancellationToken cancellationToken)
    {
        var termToUpdate = await _repositoryWrapper.TermRepository.GetSingleOrDefaultAsync(t => t.Id == request.Id);

        if (termToUpdate == null)
        {
            return Fail($"Term with id {request.Id} not found.");
        }

        _mapper.Map(request.TermDTO, termToUpdate);
        _repositoryWrapper.TermRepository.Update(termToUpdate);
        var isSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (isSuccess)
        {
            return Ok(_mapper.Map<TermDTO>(termToUpdate));
        }
        else
        {
            return Fail("Failed to update the term.");
        }
    }
}