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
using Streetcode.DAL.Entities.Sources;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Update
{
    public class UpdateSourceLinkCategoryHandler : IRequestHandler<UpdateSourceLinkCategoryCommand, Result<SourceLinkCategoryDTO>>
    {
        private IMapper _mapper;
        private ILoggerService _loggerService;
        private IRepositoryWrapper _repositoryWrapper;

        public UpdateSourceLinkCategoryHandler(IMapper mapper, ILoggerService loggerService, IRepositoryWrapper repositoryWrapper)
        {
            _mapper = mapper;
            _loggerService = loggerService;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<SourceLinkCategoryDTO>> Handle(UpdateSourceLinkCategoryCommand request, CancellationToken cancellationToken)
        {
            var categoryEntity = await _repositoryWrapper.SourceCategoryRepository
            .GetFirstOrDefaultAsync(c => c.Id == request.sourceLinkCategoryUpdate.Id);

            if (categoryEntity == null)
            {
                return Result.Fail("Category not found.");
            }

            _mapper.Map(request.sourceLinkCategoryUpdate, categoryEntity);

            var existing = await _repositoryWrapper.SourceCategoryRepository
            .GetFirstOrDefaultAsync(c => (c.Title == categoryEntity.Title || c.ImageId == categoryEntity.ImageId)
                                         && c.Id != categoryEntity.Id);
            if (existing != null)
            {
                return Result.Fail("Category with the same title or image already exists.");
            }

            _repositoryWrapper.SourceCategoryRepository.Update(categoryEntity);

            var saveResult = await _repositoryWrapper.SaveChangesAsync();

            if (saveResult <= 0)
            {
                const string errorMsg = $"Error while saving";
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _loggerService.LogInformation($"Success! SourceLinkCategory was updated.");
            var result = _mapper.Map<SourceLinkCategoryDTO>(categoryEntity);

            return Result.Ok(result);
        }
    }
}
