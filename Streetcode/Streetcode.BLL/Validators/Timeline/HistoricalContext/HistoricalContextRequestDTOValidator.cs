using FluentValidation;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Timeline.HistoricalContext
{
    public class HistoricalContextRequestDtoValidator : AbstractValidator<HistoricalContextRequestDto>
    {
        public const int MaxTitleLength = 50;

        public HistoricalContextRequestDtoValidator()
        {
            RuleFor(x => x)
                .Must(x => x.Id.HasValue || !string.IsNullOrWhiteSpace(x.Title))
                .WithMessage(Errors_Timeline.Context_MustHaveIdOrTitle)
                .Must(x => !(x.Id.HasValue && !string.IsNullOrEmpty(x.Title)))
                .WithMessage(Errors_Timeline.Context_CannotHaveBothIdAndTitle);

            RuleFor(x => x.Id)
                .GreaterThan(0)
                .When(x => x.Id.HasValue)
                .WithMessage(Errors_Validation.GreaterThan.FormatWith("Id", 0));

            RuleFor(x => x.Title)
                .MaximumLength(MaxTitleLength)
                .WithMessage(Errors_Validation.MaxLength.FormatWith("Title", MaxTitleLength))
                .Matches(@"^[a-zA-Zа-яА-ЯіІїЇєЄ\s]+$")
                .WithMessage(Errors_Validation.InvalidCharacters.FormatWith("Title"))
                .When(x => !string.IsNullOrEmpty(x.Title));
        }
    }
}
