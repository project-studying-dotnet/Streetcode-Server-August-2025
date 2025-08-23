using FluentValidation;
using Streetcode.BLL.DTO.News;

namespace Streetcode.BLL.Validators
{
    public class NewsDTOValidator : AbstractValidator<NewsDTO>
    {
        public NewsDTOValidator()
        {
            // Title validation
            RuleFor(x => x.Title)
                .NotEmpty()
                    .WithMessage("Title is required.")
                .MaximumLength(200)
                    .WithMessage("Title cannot exceed 200 characters.")
                .MinimumLength(3)
                    .WithMessage("Title must be at least 3 characters long.");

            // Text validation
            RuleFor(x => x.Text)
                .NotEmpty()
                    .WithMessage("Text content is required.")
                .MaximumLength(5000)
                    .WithMessage("Text content cannot exceed 5000 characters.")
                .MinimumLength(10)
                    .WithMessage("Text content must be at least 10 characters long.");

            // URL validation
            RuleFor(x => x.URL)
                .NotEmpty()
                    .WithMessage("URL is required.")
                .MaximumLength(255)
                    .WithMessage("URL cannot exceed 255 characters.")
                .Must(BeValidUrl)
                    .WithMessage("URL must be in a valid format.");

            // CreationDate validation
            RuleFor(x => x.CreationDate)
                .NotEmpty()
                    .WithMessage("Creation date is required.")
                .LessThanOrEqualTo(DateTime.UtcNow)
                    .WithMessage("Creation date cannot be in the future.");

            // ImageId validation (optional)
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
