using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.Term.Update;

namespace Streetcode.BLL.MediatR.Streetcode.Term.Update;

public class UpdateTermCommandValidator : AbstractValidator<UpdateTermCommand>
{
    public UpdateTermCommandValidator()
    {
        RuleFor(x => x.TermDTO.Title)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.TermDTO.Description)
            .NotEmpty()
            .MaximumLength(500);
    }
}