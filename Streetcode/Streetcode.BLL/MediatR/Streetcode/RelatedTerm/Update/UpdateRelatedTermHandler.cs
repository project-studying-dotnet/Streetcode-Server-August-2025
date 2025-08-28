using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Update
{
    public class UpdateRelatedTermHandler : IRequestHandler<UpdateRelatedTermCommand, Result<Unit>>
    {
        public Task<Result<Unit>> Handle(UpdateRelatedTermCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
