using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.FavouriteStreetcode.GetFavoritesByUserId
{
    public class GetFavoritesByUserIdHandler : IRequestHandler<GetFavoritesByUserIdQuery, Result<IEnumerable<StreetcodeDTO>>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;
        public GetFavoritesByUserIdHandler(
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<StreetcodeDTO>>> Handle(GetFavoritesByUserIdQuery request, CancellationToken cancellationToken)
        {
            var favoriteStreetcodes = await _repositoryWrapper.FavouriteStreetcodeRepository
                .GetAllAsync(fs => fs.UserId == request.UserId);

            if (favoriteStreetcodes == null)
            {
                return Result.Ok(Enumerable.Empty<StreetcodeDTO>());
            }

            var streetcodeIds = favoriteStreetcodes.Select(fs => fs.StreetcodeId).ToList();

            var streetcodes = await _repositoryWrapper.StreetcodeRepository
                .GetAllAsync(sc => streetcodeIds.Contains(sc.Id));

            if (streetcodes == null)
            {
                return Result.Ok(Enumerable.Empty<StreetcodeDTO>());
            }

            var streetcodeDtos = _mapper.Map<IEnumerable<StreetcodeDTO>>(streetcodes);

            return Result.Ok(streetcodeDtos);
        }
    }
}
