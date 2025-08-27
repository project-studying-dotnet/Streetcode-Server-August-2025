using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Image.Create;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using FactEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Update
{
    public class UpdateFactHandler : IRequestHandler<UpdateFactCommand, Result<FactDto>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;
        private readonly IMediator _mediator;

        public UpdateFactHandler(
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            ILoggerService logger,
            IMediator mediator)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Result<FactDto>> Handle(UpdateFactCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingFact = await _repositoryWrapper.FactRepository
                    .GetSingleOrDefaultAsync(f => f.Id == request.Fact.Id);

                if (existingFact == null)
                {
                    string errorMsg = $"Fact with Id {request.Fact.Id} not found!";
                    _logger.LogError(request, errorMsg);
                    return Result.Fail(errorMsg);
                }

                _mapper.Map(request.Fact, existingFact);

                if (request.Fact.NewImage is not null)
                {
                    var createImageResult = await _mediator.Send(
                        new CreateImageCommand(request.Fact.NewImage), cancellationToken);

                    if (createImageResult.IsFailed)
                    {
                        return Result.Fail(createImageResult.Errors.First().Message);
                    }

                    existingFact.ImageId = createImageResult.Value.Id;
                }

                await UpdateImageDetailsAsync(request, existingFact);

                return await UpdateFactAsync(existingFact);
            }
            catch (Exception ex)
            {
                _logger.LogError(request, ex.Message);
                return Result.Fail(ex.Message);
            }
        }

        private async Task UpdateImageDetailsAsync(UpdateFactCommand request, FactEntity existingFact)
        {
            if (!existingFact.ImageId.HasValue)
            {
                return;
            }

            var imageDetails = await _repositoryWrapper.ImageDetailsRepository
                .GetSingleOrDefaultAsync(id => id.ImageId == existingFact.ImageId.Value);

            if (imageDetails != null)
            {
                imageDetails.Alt = request.Fact.ImageDescription;
                _repositoryWrapper.ImageDetailsRepository.Update(imageDetails);
            }
            else
            {
                _repositoryWrapper.ImageDetailsRepository.Create(new ImageDetails
                {
                    ImageId = existingFact.ImageId.Value,
                    Alt = request.Fact.ImageDescription
                });
            }
        }

        private async Task<Result<FactDto>> UpdateFactAsync(FactEntity existingFact)
        {
            _repositoryWrapper.FactRepository.Update(existingFact);
            var success = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (!success)
            {
                const string errorMsg = "Failed to update a fact";
                _logger.LogError(existingFact, errorMsg);
                return Result.Fail(errorMsg);
            }

            return Result.Ok(_mapper.Map<FactDto>(existingFact));
        }
    }
}
