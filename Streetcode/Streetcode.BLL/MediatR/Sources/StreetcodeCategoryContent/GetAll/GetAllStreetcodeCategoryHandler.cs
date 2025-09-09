using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
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
                const string errorMsg = $"Cannot find any streetcodeCategoryContent";
                _loggerService.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            var dtosList = _mapper.Map<List<StreetcodeCategoryContentDTO>>(entities);

            return Result.Ok(dtosList.AsEnumerable());
        }
    }
}
