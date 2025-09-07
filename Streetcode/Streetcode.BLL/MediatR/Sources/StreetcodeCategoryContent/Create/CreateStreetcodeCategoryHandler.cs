using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Entities.Sources;

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
                const string errorMsg = $"Category with this name already exist.";
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            await _repositoryWrapper.StreetcodeCategoryContentRepository.CreateAsync(streetcodeCategoryContentEntity);

            var saveResult = await _repositoryWrapper.SaveChangesAsync();

            if (saveResult <= 0)
            {
                const string errorMsg = "Failed to save category content in database";
                _loggerService.LogError(request, errorMsg);
                return Result.Fail<StreetcodeCategoryContentDTO>(errorMsg);
            }

            _loggerService.LogInformation($"Success! StreetcodeCategoryContent was created.");
            var result = _mapper.Map<StreetcodeCategoryContentDTO>(streetcodeCategoryContentEntity);

            return Result.Ok(result);
        }
    }
}
