using FluentValidation;
using Streetcode.BLL.MediatR.Media.Image.Create;

namespace Streetcode.BLL.Validators.Media.Image;

public class CreateImageCommandValidator : AbstractValidator<CreateImageCommand>
{
    public CreateImageCommandValidator()
    {
        RuleFor(x => x.Image)
            .NotNull()
            .WithMessage("Image data is required.");

        When(x => x.Image != null, () =>
        {
            RuleFor(x => x.Image)
                .SetValidator(new ImageFileBaseCreateDTOValidator());
        });
    }
}