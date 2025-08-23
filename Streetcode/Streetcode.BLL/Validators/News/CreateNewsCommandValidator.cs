using FluentValidation;
using Streetcode.BLL.MediatR.Newss.Create;

namespace Streetcode.BLL.Validators.News
{
    public class CreateNewsCommandValidator : AbstractValidator<CreateNewsCommand>
    {
        public CreateNewsCommandValidator()
        {
            RuleFor(x => x.newNews)
                .NotNull()
                    .WithMessage("News data is required.");

            When(x => x.newNews != null, () =>
            {
                RuleFor(x => x.newNews)
                    .SetValidator(new NewsDTOValidator());
            });
        }
    }
}
