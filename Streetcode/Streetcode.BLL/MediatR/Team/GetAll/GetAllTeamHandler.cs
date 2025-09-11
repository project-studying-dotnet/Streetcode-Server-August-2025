using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Team;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Specifications.Team;

namespace Streetcode.BLL.MediatR.Team.GetAll
{
    public class GetAllTeamHandler : IRequestHandler<GetAllTeamQuery, Result<IEnumerable<TeamMemberDTO>>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public GetAllTeamHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<TeamMemberDTO>>> Handle(GetAllTeamQuery request, CancellationToken cancellationToken)
        {
            var spec = new AllTeamSpecification();

            var team = await _repositoryWrapper.TeamRepository.ListAsync(spec, cancellationToken);

            if (team is null)
            {
                string errorMsg = Errors_Common.NotFoundAny.FormatWith("team");
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            return Result.Ok(_mapper.Map<IEnumerable<TeamMemberDTO>>(team));
        }
    }
}