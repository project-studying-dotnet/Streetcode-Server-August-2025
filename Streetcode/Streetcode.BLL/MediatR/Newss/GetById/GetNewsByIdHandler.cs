using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Newss.GetById
{
    public class GetNewsByIdHandler : IRequestHandler<GetNewsByIdQuery, Result<NewsDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IBlobService _blobService;
        private readonly ILoggerService _logger;
        public GetNewsByIdHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, IBlobService blobService, ILoggerService logger)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _blobService = blobService;
            _logger = logger;
        }

        public async Task<Result<NewsDTO>> Handle(GetNewsByIdQuery request, CancellationToken cancellationToken)
        {
            var news = await _repositoryWrapper.NewsRepository.GetFirstOrDefaultAsync(
                predicate: sc => sc.Id == request.id,
                include: scl => scl.Include(sc => sc.Image));

            if (news is null)
            {
                string errorMsg = Errors_Common.NotFoundById.FormatWith("news", request.id);
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var newsDTO = _mapper.Map<NewsDTO>(news);

            if (newsDTO.Image is not null)
            {
                newsDTO.Image.Base64 = _blobService.FindFileInStorageAsBase64(newsDTO.Image.BlobName);
            }

            return Result.Ok(newsDTO);
        }
    }
}