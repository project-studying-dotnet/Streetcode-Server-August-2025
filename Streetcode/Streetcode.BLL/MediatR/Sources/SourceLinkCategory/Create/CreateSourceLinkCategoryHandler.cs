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

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create
{
    public class CreateSourceLinkCategoryHandler : IRequestHandler<CreateSourceLinkCategoryCommand, Result<SourceLinkCategoryDTO>>
    {
        private readonly IMapper _mapper;
        private readonly ILoggerService _loggerService;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public CreateSourceLinkCategoryHandler(IMapper mapper, ILoggerService loggerService, IRepositoryWrapper repositoryWrapper)
        {
            _mapper = mapper;
            _loggerService = loggerService;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<SourceLinkCategoryDTO>> Handle(CreateSourceLinkCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<DAL.Entities.Sources.SourceLinkCategory>(request.SourceLinkCategoryCreateDTO);

            var existing = await _repositoryWrapper.SourceCategoryRepository
            .GetFirstOrDefaultAsync(c => c.Title == entity.Title
                              || c.ImageId == entity.ImageId);
            if (existing != null)
            {
                return Result.Fail("Category with the same title or image already exists.");
            }

            await _repositoryWrapper.SourceCategoryRepository.CreateAsync(entity);
            var isSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (!isSuccess)
            {
                const string errorMsg = "Failed to create category";
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            _loggerService.LogInformation($"Success! SourceLinkCategory was created.");
            var categoryDto = _mapper.Map<SourceLinkCategoryDTO>(entity);

            return Result.Ok(categoryDto);
        }
    }
}
