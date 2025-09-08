using FluentValidation;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.Helpers;

namespace Streetcode.BLL.Validators.News
{
    public class NewsDTOValidator : AbstractValidator<NewsDTO>
    {
        public const int MaxTitleLength = 150;
        public const int MinTitleLength = 2;
        public const int MaxUrlLength = 100;

        public NewsDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                    .WithMessage(Errors_Validation.IsRequired.FormatWith("Title"))
                .Length(MinTitleLength, MaxTitleLength)
                    .WithMessage(Errors_Validation.LengthMustBeInRange.FormatWith("Title", MinTitleLength, MaxTitleLength));

            RuleFor(x => x.Text)
                .NotEmpty()
                    .WithMessage(Errors_Validation.IsRequired.FormatWith("Text"));

            RuleFor(x => x.URL)
                .NotEmpty()
                    .WithMessage(Errors_Validation.IsRequired.FormatWith("URL"))
                .MaximumLength(MaxUrlLength)
                    .WithMessage(Errors_Validation.MaxLength.FormatWith("URL", MaxUrlLength))
                .Must(ValidationHelper.BeValidUrl)
                    .WithMessage(Errors_Validation.InvalidNewsUrl);

            RuleFor(x => x.CreationDate)
                .NotEmpty()
                    .WithMessage(Errors_Validation.IsRequired.FormatWith("CreationDate"));

            When(x => x.ImageId.HasValue, () =>
            {
                RuleFor(x => x.ImageId)
                    .GreaterThan(0)
                        .WithMessage(Errors_Validation.Invalid.FormatWith("ImageId"));
            });
        }
    }
}
