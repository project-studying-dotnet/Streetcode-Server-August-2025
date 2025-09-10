using FluentValidation;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.AdditionalContent.Tag;

public class TagValidator : AbstractValidator<CreateTagDTO>
{
    public const int TitleMaxLength = 50;
    public TagValidator()
    {
        RuleFor(dto => dto.Title)
            .NotEmpty().WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Title"))
            .MaximumLength(TitleMaxLength).WithMessage(Errors_Validation.MaxLength.FormatWith("Title", TitleMaxLength));
    }
}
