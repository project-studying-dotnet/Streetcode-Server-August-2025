using FluentValidation;
using Streetcode.BLL.DTO.News;

namespace Streetcode.BLL.Validators.News
{
    public class NewsDTOValidator : AbstractValidator<NewsDTO>
    {
        public NewsDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                    .WithMessage("Title is required.")
                .MinimumLength(2)
                    .WithMessage("Title must be at least 2 characters long.")
                .MaximumLength(150)
                    .WithMessage("Title cannot exceed 150 characters.");

            RuleFor(x => x.Text)
                .NotEmpty()
                    .WithMessage("Text content is required.");

            RuleFor(x => x.URL)
                .NotEmpty()
                    .WithMessage("URL is required.")
                .MaximumLength(100)
                    .WithMessage("URL cannot exceed 100 characters.")
                .Must(BeValidUrl)
                    .WithMessage("URL must be in a valid format.");

            RuleFor(x => x.CreationDate)
                .NotEmpty()
                    .WithMessage("Creation date is required.");

            When(x => x.ImageId.HasValue, () =>
            {
                RuleFor(x => x.ImageId)
                    .GreaterThan(0)
                        .WithMessage("Image ID must be greater than 0.");
            });
        }

        private static bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
