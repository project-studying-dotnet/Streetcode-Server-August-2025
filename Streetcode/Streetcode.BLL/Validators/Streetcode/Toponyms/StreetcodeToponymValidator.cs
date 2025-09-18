using FluentValidation;
using Streetcode.BLL.DTO.Toponyms;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Streetcode.Toponyms;

public class StreetcodeToponymValidator : AbstractValidator<StreetcodeToponymCreateUpdateDTO>
{
    public const int StreetNameMaxLength = 150;

    public StreetcodeToponymValidator()
    {
        RuleFor(dto => dto.StreetName)
            .NotEmpty()
                .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("StreetName"))
            .MaximumLength(StreetNameMaxLength)
                .WithMessage(Errors_Validation.MaxLength.FormatWith("StreetName", StreetNameMaxLength));

        RuleFor(dto => dto.ModelState)
            .IsInEnum()
                .WithMessage(Errors_Validation.Invalid.FormatWith("ModelState"));
    }
}