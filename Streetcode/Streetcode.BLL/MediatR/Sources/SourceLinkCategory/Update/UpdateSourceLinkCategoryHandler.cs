using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Update
{
    public class UpdateSourceLinkCategoryHandler : IRequestHandler<UpdateSourceLinkCategoryCommand, Result<SourceLinkCategoryDTO>>
    {
        private readonly IMapper _mapper;
        private readonly ILoggerService _loggerService;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public UpdateSourceLinkCategoryHandler(IMapper mapper, ILoggerService loggerService, IRepositoryWrapper repositoryWrapper)
        {
            _mapper = mapper;
            _loggerService = loggerService;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<SourceLinkCategoryDTO>> Handle(UpdateSourceLinkCategoryCommand request, CancellationToken cancellationToken)
        {
            var categoryEntity = await _repositoryWrapper.SourceCategoryRepository
            .GetFirstOrDefaultAsync(c => c.Id == request.SourceLinkCategoryUpdate.Id);

            if (categoryEntity == null)
            {
                string errorMsg = Errors_Common.NotFoundById.FormatWith("SourceLinkCategory", request.SourceLinkCategoryUpdate.Id);
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _mapper.Map(request.SourceLinkCategoryUpdate, categoryEntity);

            var existing = await _repositoryWrapper.SourceCategoryRepository
            .GetFirstOrDefaultAsync(c => (c.Title == categoryEntity.Title || c.ImageId == categoryEntity.ImageId)
                                         && c.Id != categoryEntity.Id);
            if (existing != null)
            {
                string errorMsg = Errors_Sources.AlreadyExistByTitleOrImage;
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _repositoryWrapper.SourceCategoryRepository.Update(categoryEntity);

            var saveResult = await _repositoryWrapper.SaveChangesAsync();

            if (saveResult <= 0)
            {
                string errorMsg = Errors_Common.FailedToUpdate.FormatWith("SourceLinkCategory");
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _loggerService.LogInformation($"Success! SourceLinkCategory was updated.");
            var result = _mapper.Map<SourceLinkCategoryDTO>(categoryEntity);

            return Result.Ok(result);
        }
    }
}
