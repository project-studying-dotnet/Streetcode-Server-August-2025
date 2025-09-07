using System.Security.Cryptography;
using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Interfaces.Logging;

namespace Streetcode.BLL.MediatR.Newss.GetNewsAndLinksByUrl
{
    public class GetNewsAndLinksByUrlHandler : IRequestHandler<GetNewsAndLinksByUrlQuery, Result<NewsDTOWithURLs>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IBlobService _blobService;
        private readonly ILoggerService _logger;
        public GetNewsAndLinksByUrlHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, IBlobService blobService, ILoggerService logger)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _blobService = blobService;
            _logger = logger;
        }

        public async Task<Result<NewsDTOWithURLs>> Handle(GetNewsAndLinksByUrlQuery request, CancellationToken cancellationToken)
        {
            var newsEntity = await _repositoryWrapper.NewsRepository.GetFirstOrDefaultAsync(
                predicate: sc => sc.URL == request.url,
                include: scl => scl.Include(sc => sc.Image));

            if (newsEntity is null)
            {
                string errorMsg = $"No news by entered Url - {request.url}";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            var newsDTO = _mapper.Map<NewsDTO>(newsEntity);

            if (newsDTO.Image is not null)
            {
                newsDTO.Image.Base64 = _blobService.FindFileInStorageAsBase64(newsDTO.Image.BlobName);
            }

            var orderedNews = (await _repositoryWrapper.NewsRepository.GetAllAsync())
                .OrderByDescending(n => n.CreationDate)
                .Select(n => new { n.Id, n.URL, n.Title })
                .ToList();

            var index = orderedNews.FindIndex(x => x.Id == newsDTO.Id);
            if (index == -1)
            {
                string errorMsg = $"News with Id {newsDTO.Id} not found in ordered list";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            string? prevNewsLink = index > 0 ? orderedNews[index - 1].URL : null;
            string? nextNewsLink = index < orderedNews.Count - 1 ? orderedNews[index + 1].URL : null;

            // Random news selection (excluding current)
            RandomNewsDTO randomNews = new();
            var candidates = orderedNews.Where(n => n.Id != newsDTO.Id).ToList();
            if (candidates.Any())
            {
                using (var rng = RandomNumberGenerator.Create())
                {
                    byte[] bytes = new byte[4];
                    rng.GetBytes(bytes);
                    int value = BitConverter.ToInt32(bytes, 0) & int.MaxValue;
                    index = value % candidates.Count;
                }

                var pick = candidates[index];
                randomNews.RandomNewsUrl = pick.URL;
                randomNews.Title = pick.Title;
            }

            var result = new NewsDTOWithURLs
            {
                News = newsDTO,
                PrevNewsUrl = prevNewsLink,
                NextNewsUrl = nextNewsLink,
                RandomNews = randomNews
            };

            return Result.Ok(result);
        }
    }
}
