using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update
{
    public class UpdateStreetcodeCategoryContentHandler : IRequestHandler<UpdateStreetcodeCategoryContentCommand, Result<StreetcodeCategoryContentDTO>>
    {
        private readonly IMapper _mapper;
        private readonly ILoggerService _loggerService;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public UpdateStreetcodeCategoryContentHandler(IMapper mapper, ILoggerService loggerService, IRepositoryWrapper repositoryWrapper)
        {
            _mapper = mapper;
            _loggerService = loggerService;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<StreetcodeCategoryContentDTO>> Handle(UpdateStreetcodeCategoryContentCommand request, CancellationToken cancellationToken)
        {
            var streetcodeCategoryContent = await _repositoryWrapper.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(t => t.Id == request.CategoryContentUpdateDTO.Id);

            if (streetcodeCategoryContent == null)
            {
                string errorMsg = Errors_Common.NotFoundById.FormatWith("StreetcodeCategoryContent", request.CategoryContentUpdateDTO.Id);
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _mapper.Map(request.CategoryContentUpdateDTO, streetcodeCategoryContent);

            var exists = await _repositoryWrapper.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(x =>
                x.Id != request.CategoryContentUpdateDTO.Id &&
                x.SourceLinkCategoryId == request.CategoryContentUpdateDTO.SourceLinkCategoryId &&
                x.StreetcodeId == request.CategoryContentUpdateDTO.StreetcodeId);

            if (exists != null)
            {
                string errorMsg = Errors_Sources.AlreadyExistByStreetcodeAndSourceLinkCategory;
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _repositoryWrapper.StreetcodeCategoryContentRepository.Update(streetcodeCategoryContent);

            var saveResult = await _repositoryWrapper.SaveChangesAsync();

            if (saveResult <= 0)
            {
                string errorMsg = Errors_Common.FailedToUpdate.FormatWith("StreetcodeCategoryContent", request.CategoryContentUpdateDTO.Id);
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _loggerService.LogInformation($"Success! StreetcodeCategoryContent was updated.");

            var result = _mapper.Map<StreetcodeCategoryContentDTO>(streetcodeCategoryContent);
            return Result.Ok(result);
        }
    }
}
