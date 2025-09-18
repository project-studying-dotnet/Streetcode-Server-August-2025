using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.GetAll
{
    public class GetAllStreetcodeCategoryHandler : IRequestHandler<GetAllStreetcodeCategoryContentQuery, Result<IEnumerable<StreetcodeCategoryContentDTO>>>
    {
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public GetAllStreetcodeCategoryHandler(ILoggerService loggerService, IMapper mapper, IRepositoryWrapper repositoryWrapper)
        {
            _loggerService = loggerService;
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<IEnumerable<StreetcodeCategoryContentDTO>>> Handle(GetAllStreetcodeCategoryContentQuery request, CancellationToken cancellationToken)
        {
            var entities = await _repositoryWrapper.StreetcodeCategoryContentRepository.GetAllAsync();

            if (entities == null)
            {
                string errorMsg = Errors_Common.NotFoundAny.FormatWith("StreetcodeCategoryContent");
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            var dtosList = _mapper.Map<List<StreetcodeCategoryContentDTO>>(entities);

            return Result.Ok(dtosList.AsEnumerable());
        }
    }
}
