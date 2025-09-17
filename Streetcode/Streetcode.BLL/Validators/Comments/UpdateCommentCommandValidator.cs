using FluentValidation;
using Streetcode.BLL.MediatR.Comments.Update;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;

namespace Streetcode.BLL.Validators.Comments;

public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public const int MaxTextLength = 1000;

    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.Comment)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequired.FormatWith("Comment"));

        When(x => x.Comment != null, () =>
        {
            RuleFor(x => x.Comment.Id)
                .GreaterThan(0)
                .WithMessage(Errors_Validation.IsRequired.FormatWith("CommentId"));

            RuleFor(x => x.Comment.Text)
                .NotEmpty()
                .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Text"))
                .MaximumLength(MaxTextLength)
                .WithMessage(Errors_Validation.MaxLength.FormatWith("Text", MaxTextLength));
        });
    }
}