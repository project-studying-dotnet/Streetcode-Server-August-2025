using FluentValidation;
using Streetcode.BLL.MediatR.Newss.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.News
{
    public class CreateNewsCommandValidator : AbstractValidator<CreateNewsCommand>
    {
        public CreateNewsCommandValidator()
        {
            RuleFor(x => x.NewNews)
                .NotNull()
                    .WithMessage(Errors_Validation.IsRequiredData.FormatWith("News"));

            When(x => x.NewNews != null, () =>
            {
                RuleFor(x => x.NewNews)
                    .SetValidator(new NewsDTOValidator());
            });
        }
    }
}
