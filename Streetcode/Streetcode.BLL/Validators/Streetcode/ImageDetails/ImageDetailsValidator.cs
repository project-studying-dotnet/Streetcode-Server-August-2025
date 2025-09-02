using FluentValidation;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.Validators.Streetcode.ImageDetails;

public class ImageDetailsValidator : AbstractValidator<ImageDetailsDto>
{
    public const int TitleMaxLength = 100;
    public const int AltMaxLength = 200;

    private readonly IRepositoryWrapper _repositoryWrapper;

    public ImageDetailsValidator(IRepositoryWrapper repositoryWrapper)
    {
        _repositoryWrapper = repositoryWrapper;

        RuleFor(dto => dto)
            .MustAsync(BeUniqueImageIdWithImageDetails)
            .WithMessage("An ImageDetails entry with the same ImageId already exists.")
            .When(dto => dto.ImageId != 0);

        RuleFor(dto => dto.Title)
            .MaximumLength(TitleMaxLength)
            .WithMessage($"Title cannot exceed {TitleMaxLength} characters.");

        RuleFor(dto => dto.Alt)
            .MaximumLength(AltMaxLength)
            .WithMessage($"Alt text cannot exceed {AltMaxLength} characters.");

        RuleFor(dto => dto.ImageId)
            .MustAsync(HasExistingImage)
            .WithMessage("The specified ImageId does not exist.");
    }

    private async Task<bool> BeUniqueImageIdWithImageDetails(ImageDetailsDto imageDetails, CancellationToken cancellationToken)
    {
        var existingImageDetails = await _repositoryWrapper.ImageDetailsRepository
            .GetFirstOrDefaultAsync(id => id.ImageId == imageDetails.ImageId);
        return existingImageDetails == null || existingImageDetails.Id == imageDetails.Id;
    }

    private async Task<bool> HasExistingImage(int imageId, CancellationToken token)
    {
        var image = await _repositoryWrapper.ImageRepository.GetFirstOrDefaultAsync(i => i.Id == imageId);
        return image != null;
    }
}
