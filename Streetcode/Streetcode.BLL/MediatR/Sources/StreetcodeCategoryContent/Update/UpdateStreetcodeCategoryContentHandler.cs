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

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update
{
    public class UpdateStreetcodeCategoryContentHandler : IRequestHandler<UpdateStreetcodeCategoryContentCommand, Result<StreetcodeCategoryContentDTO>>
    {
        private IMapper _mapper;
        private ILoggerService _loggerService;
        private IRepositoryWrapper _repositoryWrapper;

        public UpdateStreetcodeCategoryContentHandler(IMapper mapper, ILoggerService loggerService, IRepositoryWrapper repositoryWrapper)
        {
            _mapper = mapper;
            _loggerService = loggerService;
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<Result<StreetcodeCategoryContentDTO>> Handle(UpdateStreetcodeCategoryContentCommand request, CancellationToken cancellationToken)
        {
            var streetcodeCategoryContent = await _repositoryWrapper.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(t => t.Id == request.categoryContentUpdateDTO.Id);

            if (streetcodeCategoryContent == null)
            {
                const string errorMsg = $"StreetcodeCategoryContent don`t exist.";
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _mapper.Map(request.categoryContentUpdateDTO, streetcodeCategoryContent);

            var exists = await _repositoryWrapper.StreetcodeCategoryContentRepository
                .GetFirstOrDefaultAsync(x =>
                x.Id != request.categoryContentUpdateDTO.Id &&
                x.SourceLinkCategoryId == request.categoryContentUpdateDTO.SourceLinkCategoryId &&
                x.StreetcodeId == request.categoryContentUpdateDTO.StreetcodeId);

            if(exists != null)
            {
                const string errorMsg = $"Category with the same Streetcode and SourceLinkCategory already exists";
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _repositoryWrapper.StreetcodeCategoryContentRepository.Update(streetcodeCategoryContent);

            var saveResult = await _repositoryWrapper.SaveChangesAsync();

            if(saveResult <= 0)
            {
                const string errorMsg = $"Error while saving";
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            _loggerService.LogInformation($"Success! StreetcodeCategoryContent was updated.");

            var result = _mapper.Map<StreetcodeCategoryContentDTO>(streetcodeCategoryContent);
            return Result.Ok(result);
        }
    }
}
