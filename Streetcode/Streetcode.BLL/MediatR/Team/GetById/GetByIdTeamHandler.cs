using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Specifications.Team;

namespace Streetcode.BLL.MediatR.Team.GetById
{
    public class GetByIdTeamHandler : IRequestHandler<GetByIdTeamQuery, Result<TeamMemberDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public GetByIdTeamHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<TeamMemberDTO>> Handle(GetByIdTeamQuery request, CancellationToken cancellationToken)
        {
            var spec = new TeamByIdSpecification(request.Id);

            var team = await _repositoryWrapper.TeamRepository.GetBySpecAsync(spec, cancellationToken);

            if (team is null)
            {
                string errorMsg = Errors_Common.NotFoundById.FormatWith("team", request.Id);
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            return Result.Ok(_mapper.Map<TeamMemberDTO>(team));
        }
    }
}