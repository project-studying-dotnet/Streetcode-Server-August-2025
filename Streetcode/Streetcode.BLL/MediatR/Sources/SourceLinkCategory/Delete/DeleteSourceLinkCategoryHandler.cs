using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.Delete
{
    public class DeleteSourceLinkCategoryHandler : IRequestHandler<DeleteSourceLinkCategoryCommand, Result<SourceLinkCategoryDTO>>
    {
        private ILoggerService _loggerService;
        private IMapper _mapper;
        private IRepositoryWrapper _repositoryWrapper;

        public DeleteSourceLinkCategoryHandler(ILoggerService loggerService, IMapper mapper, IRepositoryWrapper repositoryWrapper)
        {
            _loggerService = loggerService;
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<SourceLinkCategoryDTO>> Handle(DeleteSourceLinkCategoryCommand request, CancellationToken cancellationToken)
        {
            var streetcodeCategoryContent = await _repositoryWrapper.SourceCategoryRepository.GetFirstOrDefaultAsync(t => t.Id == request.id);

            if (streetcodeCategoryContent == null)
            {
                string errorMsg = Errors_Common.NotFoundById.FormatWith("SourceLinkCategory", request.id);
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _repositoryWrapper.SourceCategoryRepository.Delete(streetcodeCategoryContent);

            var saveResult = await _repositoryWrapper.SaveChangesAsync();

            if (saveResult == 0)
            {
                string errorMsg = Errors_Common.FailedToDelete.FormatWith("SourceLinkCategory");
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _loggerService.LogInformation($"Success! SourceLinkCategory was deleted.");
            var categoryDto = _mapper.Map<SourceLinkCategoryDTO>(streetcodeCategoryContent);

            return Result.Ok(categoryDto);
        }
    }
}
