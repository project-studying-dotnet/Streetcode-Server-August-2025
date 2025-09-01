using FluentValidation;
using Streetcode.BLL.DTO.Timeline.HistoricalContext;

namespace Streetcode.BLL.Validators.Timeline.HistoricalContext
{
    public class HistoricalContextRequestDTOValidator : AbstractValidator<HistoricalContextRequestDTO>
    {
        public HistoricalContextRequestDTOValidator()
        {
            RuleFor(x => x)
                .Must(x => x.Id.HasValue || !string.IsNullOrEmpty(x.Title))
                .WithMessage("Context must have either an ID or a title.")
                .Must(x => !(x.Id.HasValue && !string.IsNullOrEmpty(x.Title)))
                .WithMessage("Cannot provide both an ID and a title for one context.");

            RuleFor(x => x.Id)
                .GreaterThan(0)
                .When(x => x.Id.HasValue)
                .WithMessage("ID must be greater than zero.");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required.")
                .MaximumLength(50)
                .WithMessage("Title cannot exceed 50 characters.")
                .Matches(@"^[a-zA-Zа-яА-ЯіІїЇєЄ\s]+$")
                .WithMessage("Title can only contain letters and spaces.")
                .When(x => !string.IsNullOrEmpty(x.Title));
        }
    }
}
