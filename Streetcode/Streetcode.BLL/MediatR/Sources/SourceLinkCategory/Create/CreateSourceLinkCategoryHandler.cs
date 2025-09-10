using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Create
{
    public class CreateSourceLinkCategoryHandler : IRequestHandler<CreateSourceLinkCategoryCommand, Result<SourceLinkCategoryDTO>>
    {
        private IMapper _mapper;
        private ILoggerService _loggerService;
        private IRepositoryWrapper _repositoryWrapper;

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
                string errorMsg = Errors_Sources.AlreadyExistByTitleOrImage;
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            await _repositoryWrapper.SourceCategoryRepository.CreateAsync(entity);
            var isSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (!isSuccess)
            {
                string errorMsg = Errors_Common.FailedToCreate.FormatWith("SourceLinkCategory");
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _loggerService.LogInformation($"Success! SourceLinkCategory was created.");
            var categoryDto = _mapper.Map<SourceLinkCategoryDTO>(entity);

            return Result.Ok(categoryDto);
        }
    }
}
