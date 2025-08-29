using FluentValidation;
using Streetcode.BLL.MediatR.Newss.Update;

namespace Streetcode.BLL.Validators.News;

public class UpdateNewsCommandValidator : AbstractValidator<UpdateNewsCommand>
{
    public UpdateNewsCommandValidator()
    {
        RuleFor(x => x.news)
            .NotNull()
            .WithMessage("News data is required.");

        When(x => x.news != null, () =>
        {
            RuleFor(x => x.news)
                .SetValidator(new NewsDTOValidator());

            RuleFor(x => x.news.Id)
                .GreaterThan(0)
                .WithMessage("Id must be provided and greater than 0 when updating news.");
        });
    }
}