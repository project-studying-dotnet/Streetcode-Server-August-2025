using FluentValidation;
using Streetcode.BLL.MediatR.Newss.Update;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.News;

public class UpdateNewsCommandValidator : AbstractValidator<UpdateNewsCommand>
{
    public UpdateNewsCommandValidator()
    {
        RuleFor(x => x.news)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequiredData.FormatWith("News"));

        When(x => x.news != null, () =>
        {
            RuleFor(x => x.news)
                .SetValidator(new NewsDTOValidator());

            RuleFor(x => x.news.Id)
                .GreaterThan(0)
                .WithMessage(Errors_Validation.Invalid.FormatWith("Id"));
        });
    }
}