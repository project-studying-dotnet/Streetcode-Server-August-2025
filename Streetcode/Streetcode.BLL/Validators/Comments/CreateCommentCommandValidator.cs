using FluentValidation;
using Streetcode.BLL.MediatR.Comments.Create;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Comments;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public const int MaxTextLength = 1000;

    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.NewComment)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequired.FormatWith("NewComment"));

        RuleFor(x => x.NewComment.Text)
            .NotEmpty()
            .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Text"))
            .MaximumLength(MaxTextLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("Text"));

        RuleFor(x => x.NewComment.UserId)
            .GreaterThan(0)
            .WithMessage(Errors_Validation.IsRequired.FormatWith("UserId"));

        RuleFor(x => x.NewComment.StreetcodeId)
            .GreaterThan(0)
            .WithMessage(Errors_Validation.IsRequired.FormatWith("StreetcodeId"));
    }
}
