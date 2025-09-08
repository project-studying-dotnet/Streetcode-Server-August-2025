using FluentValidation;
using Streetcode.BLL.MediatR.Media.Image.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Media.Image;

public class CreateImageCommandValidator : AbstractValidator<CreateImageCommand>
{
    public CreateImageCommandValidator()
    {
        RuleFor(x => x.Image)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequired.FormatWith("Image"));

        When(x => x.Image != null, () =>
        {
            RuleFor(x => x.Image)
                .SetValidator(new ImageFileBaseCreateDTOValidator());
        });
    }
}