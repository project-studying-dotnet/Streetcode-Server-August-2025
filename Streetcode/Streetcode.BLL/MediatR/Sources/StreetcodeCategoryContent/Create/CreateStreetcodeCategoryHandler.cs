using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create
{
    public class CreateStreetcodeCategoryHandler : IRequestHandler<CreateStreetcodeCategoryContentCommand, Result<StreetcodeCategoryContentDTO>>
    {
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public CreateStreetcodeCategoryHandler(ILoggerService loggerService, IMapper mapper, IRepositoryWrapper repositoryWrapper)
        {
            _loggerService = loggerService;
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<StreetcodeCategoryContentDTO>> Handle(CreateStreetcodeCategoryContentCommand request, CancellationToken cancellationToken)
        {
            var streetcodeCategoryContentEntity = _mapper.Map<DAL.Entities.Sources.StreetcodeCategoryContent>(request.CreateCategoryContentDto);

            var isDuplicate = await _repositoryWrapper.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(predicate: c =>
            c.StreetcodeId == streetcodeCategoryContentEntity.StreetcodeId &&
            c.SourceLinkCategoryId == streetcodeCategoryContentEntity.SourceLinkCategoryId);

            if (isDuplicate != null)
            {
                string errorMsg = Errors_Sources.AlreadyExistWithName;
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            await _repositoryWrapper.StreetcodeCategoryContentRepository.CreateAsync(streetcodeCategoryContentEntity);

            var saveResult = await _repositoryWrapper.SaveChangesAsync();

            if (saveResult <= 0)
            {
                string errorMsg = Errors_Common.FailedToCreate.FormatWith("category content");
                _loggerService.LogError(request, errorMsg);
                return Result.Fail<StreetcodeCategoryContentDTO>(errorMsg);
            }

            _loggerService.LogInformation($"Success! StreetcodeCategoryContent was created.");
            var result = _mapper.Map<StreetcodeCategoryContentDTO>(streetcodeCategoryContentEntity);

            return Result.Ok(result);
        }
    }
}
