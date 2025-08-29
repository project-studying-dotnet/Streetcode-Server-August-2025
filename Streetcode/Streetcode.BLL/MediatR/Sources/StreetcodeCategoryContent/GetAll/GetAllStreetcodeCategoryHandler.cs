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
        private ILoggerService _loggerService;
        private IMapper _mapper;
        private IRepositoryWrapper _repositoryWrapper;

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

            var dtosList = new List<StreetcodeCategoryContentDTO>();
            foreach (var entity in entities)
            {
                var dto = _mapper.Map<StreetcodeCategoryContentDTO>(entity);
                dtosList.Add(dto);
            }

            return Result.Ok(dtosList.AsEnumerable());
        }
    }
}
