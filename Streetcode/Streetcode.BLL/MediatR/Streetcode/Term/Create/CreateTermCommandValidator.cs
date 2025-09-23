using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.Term.Create;
using Streetcode.BLL.DTO.Streetcode.TextContent;

namespace Streetcode.BLL.MediatR.Streetcode.Term.Create;

public class CreateTermCommandValidator : AbstractValidator<CreateTermCommand>
{
    public CreateTermCommandValidator()
    {
        RuleFor(x => x.TermDTO.Title)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.TermDTO.Description)
            .NotEmpty()
            .MaximumLength(500);
    }
}