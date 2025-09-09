using FluentValidation;
using Streetcode.BLL.MediatR.Media.Video.Create;

namespace Streetcode.BLL.Validator.Media.Video;

public sealed class CreateVideoCommandValidator : AbstractValidator<CreateVideoCommand>
{
    public CreateVideoCommandValidator()
    {
        RuleFor(c => c.CreateVideoRequest.Title)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.CreateVideoRequest.Url)
            .NotEmpty()
            .Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute));
    }
}
