using FluentValidation;
using Streetcode.BLL.MediatR.Newss.Create;

namespace Streetcode.BLL.Validators
{
    public class CreateNewsCommandValidator : AbstractValidator<CreateNewsCommand>
    {
        public CreateNewsCommandValidator()
        {
            // Validate the NewsDTO property
            RuleFor(x => x.newNews)
                .NotNull()
                    .WithMessage("News data is required.");

            // When NewsDTO is not null, apply NewsDTO validation rules
            When(x => x.newNews != null, () =>
            {
                RuleFor(x => x.newNews)
                    .SetValidator(new NewsDTOValidator());
            });
        }
    }
}
