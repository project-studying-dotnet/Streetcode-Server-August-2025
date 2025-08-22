using FluentValidation;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.DAL.Enums;

namespace Streetcode.BLL.Validators.Streetcode;

public class BaseStreetcodeValidator : AbstractValidator<StreetcodeCreateUpdateDTO>
{
    public const int IndexMaxValue = 9999;
    public const int IndexMinValue = 1;
    public const int FirstNameMaxLength = 50;
    public const int LastNameMaxLength = 50;
    public const int TitleMaxLength = 100;
    public const int AliasMaxLength = 33;
    public const int TransliterationUrlMaxLength = 100;
    public const int DateStringMaxLength = 100;
    public const int TeaserMaxLength = 520;
    public const int TeaserMaxLengthWithNewLine = 455;

    public BaseStreetcodeValidator()
    {
        RuleFor(dto => dto.Index)
            .NotNull().WithMessage("Index is required.")
            .InclusiveBetween(IndexMinValue, IndexMaxValue).WithMessage($"Index must be between {IndexMinValue} and {IndexMaxValue}.");

        RuleFor(dto => dto.FirstName)
            .MaximumLength(FirstNameMaxLength).WithMessage($"First name cannot exceed {FirstNameMaxLength} characters.");

        RuleFor(dto => dto.LastName)
            .MaximumLength(LastNameMaxLength).WithMessage($"Last name cannot exceed {LastNameMaxLength} characters.");

        RuleFor(dto => dto.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(TitleMaxLength).WithMessage($"Title cannot exceed {TitleMaxLength} characters.");

        RuleFor(dto => dto.Alias)
            .MaximumLength(AliasMaxLength).WithMessage($"Alias cannot exceed {AliasMaxLength} characters.");

        RuleFor(dto => dto.TransliterationUrl)
            .NotEmpty().WithMessage("Transliteration URL is required.")
            .MaximumLength(TransliterationUrlMaxLength).WithMessage($"Transliteration URL cannot exceed {TransliterationUrlMaxLength} characters.")
            .Matches(@"^[a-z0-9-]*$").Matches("Transliteration URL can only contain lowercase letters, numbers, and hyphens.");

        RuleFor(dto => dto.DateString)
            .NotEmpty().WithMessage("Date string is required.")
            .MaximumLength(DateStringMaxLength).WithMessage($"Date string cannot exceed {DateStringMaxLength} characters.")
            .Matches(@"^[0-9а-яА-ЯіїєґІЇЄҐ\s\(\)\-\–]+$")
            .WithMessage("Date string can only contain numbers, Ukrainian letters, spaces, parentheses, and hyphens.");

        RuleFor(dto => dto.Teaser)
            .NotEmpty().WithMessage("Teaser is required.")
            .Must(BeValidTeaserLength)
            .WithMessage($"Teaser cannot exceed {TeaserMaxLength} characters, or {TeaserMaxLengthWithNewLine} characters if it contains a newline.");

        RuleFor(dto => dto.StreetcodeType)
            .NotNull().WithMessage("Streetcode type is required.")
            .IsInEnum().WithMessage("Invalid streetcode type.");

        RuleFor(dto => dto.Status)
            .NotNull().WithMessage("Streetcode status is required.")
            .IsInEnum().WithMessage("Invalid streetcode status.");

        RuleFor(dto => dto)
            .Must(dto => string.IsNullOrEmpty(dto.FirstName) && string.IsNullOrEmpty(dto.LastName))
            .When(dto => dto.StreetcodeType == StreetcodeType.Event)
            .WithMessage("First name and Last name must be empty for Event streetcode type.");

        RuleFor(dto => dto.ImagesDetails)
            .Must(HaveExactlyOneBlackAndWhite)
            .WithMessage("There must be exactly one black and white image.");

        RuleFor(dto => dto.ImagesDetails)
            .Must(HaveAtMostOneAnimation)
            .WithMessage("There can be at most one animation image.");

        RuleFor(dto => dto.ImagesDetails)
            .Must(HaveAtMostOneRelatedFigure)
            .WithMessage("There can be at most one related figure image.");
    }

    private bool BeValidTeaserLength(string? teaser)
    {
        if (string.IsNullOrEmpty(teaser))
        {
            return true;
        }

        bool containsNewLine = teaser.Contains('\n');
        int maxLength = containsNewLine ? TeaserMaxLengthWithNewLine : TeaserMaxLength;

        return teaser.Length <= maxLength;
    }

    private bool HaveExactlyOneBlackAndWhite(IEnumerable<ImageDetailsDto> images)
        => images is not null && images.Count(i => i.Alt == $"{(int)ImageAssigment.Blackandwhite}") == 1;

    private bool HaveAtMostOneAnimation(IEnumerable<ImageDetailsDto> images)
        => images is null || images.Count(i => i.Alt == $"{(int)ImageAssigment.Animation}") <= 1;

    private bool HaveAtMostOneRelatedFigure(IEnumerable<ImageDetailsDto> images)
        => images is null || images.Count(i => i.Alt == $"{(int)ImageAssigment.Relatedfigure}") <= 1;
}
