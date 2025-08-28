using FluentValidation;
using Streetcode.BLL.MediatR.Newss.Create;

namespace Streetcode.BLL.Validators.News
{
    public class CreateNewsCommandValidator : AbstractValidator<CreateNewsCommand>
    {
        public CreateNewsCommandValidator()
        {
            RuleFor(x => x.NewNews)
                .NotNull()
                    .WithMessage("News data is required.");

            When(x => x.NewNews != null, () =>
            {
                RuleFor(x => x.NewNews)
                    .SetValidator(new NewsDTOValidator());
            });
        }
    }
}
