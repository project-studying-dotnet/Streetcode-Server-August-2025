using FluentValidation;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Validators.Streetcode.Toponyms;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.ArtGallery;
using Streetcode.BLL.Validators.Media.Image.Art;
using Streetcode.BLL.Validators.Streetcode.ImageDetails;
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

    public BaseStreetcodeValidator(
        StreetcodeArtSlideValidator streetcodeArtSlideValidator,
        ArtCreateUpdateDTOValidator artCreateUpdateDTOValidator,
        ImageDetailsValidator imageDetailsValidator,
        StreetcodeToponymValidator streetcodeToponymValidator)
    {
        RuleFor(dto => dto.Index)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequired.FormatWith("Index"))
            .InclusiveBetween(IndexMinValue, IndexMaxValue)
            .WithMessage(Errors_Validation.MustBeBetween.FormatWith("Index", IndexMinValue, IndexMaxValue));

        RuleFor(dto => dto.FirstName)
            .MaximumLength(FirstNameMaxLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("FirstName", FirstNameMaxLength));

        RuleFor(dto => dto.LastName)
            .MaximumLength(LastNameMaxLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("LastName", LastNameMaxLength));

        RuleFor(dto => dto.Title)
            .NotEmpty()
            .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("Title"))
            .MaximumLength(TitleMaxLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("Title", TitleMaxLength));

        RuleFor(dto => dto.Alias)
            .MaximumLength(AliasMaxLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("Alias", AliasMaxLength));

        RuleFor(dto => dto.TransliterationUrl)
            .NotEmpty()
            .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("TransliterationUrl"))
            .MaximumLength(TransliterationUrlMaxLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("TransliterationUrl", TransliterationUrlMaxLength))
            .Matches(@"^[a-z0-9-]*$")
            .WithMessage(Errors_Validation.TransliterationUrlFormat);

        RuleFor(dto => dto.DateString)
            .NotEmpty()
            .WithMessage(Errors_Validation.CannotBeEmpty.FormatWith("DateString"))
            .MaximumLength(DateStringMaxLength)
            .WithMessage(Errors_Validation.MaxLength.FormatWith("DateString", DateStringMaxLength))
            .Matches(@"^[0-9а-яА-ЯіїєґІЇЄҐ\s\(\)\-\–]+$")
            .WithMessage(Errors_Validation.DateStringFormat);

        RuleFor(dto => dto.Teaser)
            .NotEmpty()
            .WithMessage("Teaser is required.")
            .Must(BeValidTeaserLength)
            .WithMessage($"Teaser cannot exceed {TeaserMaxLength} characters, or {TeaserMaxLengthWithNewLine} characters if it contains a newline.");

        RuleFor(dto => dto.StreetcodeType)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequired.FormatWith("StreetcodeType"))
            .IsInEnum()
            .WithMessage(Errors_Validation.Invalid.FormatWith("StreetcodeType"));

        RuleFor(dto => dto.Status)
            .NotNull()
            .WithMessage(Errors_Validation.IsRequired.FormatWith("StreetcodeStatus"))
            .IsInEnum()
            .WithMessage(Errors_Validation.Invalid.FormatWith("StreetcodeStatus"));

        RuleFor(dto => dto)
            .Must(dto => string.IsNullOrEmpty(dto.FirstName) && string.IsNullOrEmpty(dto.LastName))
            .When(dto => dto.StreetcodeType == StreetcodeType.Event)
            .WithMessage(Errors_Validation.EventStreetcodeCannotHasFirstName);

        RuleFor(dto => dto.ImagesDetails)
            .Must(HaveExactlyOneBlackAndWhite)
            .WithMessage(Errors_Validation.MustContainExactlyOneBlackAndWhiteImage);

        RuleFor(dto => dto.ImagesDetails)
            .Must(HaveAtMostOneAnimation)
            .WithMessage(Errors_Validation.MustContainAtMostOneColoredImage);

        RuleFor(dto => dto.ImagesDetails)
            .Must(HaveAtMostOneRelatedFigure)
            .WithMessage(Errors_Validation.MustContainAtMostOneRelatedFigureImage);

        RuleForEach(dto => dto.Toponyms)
            .SetValidator(streetcodeToponymValidator);

        RuleForEach(dto => dto.ImagesDetails)
            .SetValidator(imageDetailsValidator);

        RuleForEach(dto => dto.StreetcodeArtSlides)
            .SetValidator(streetcodeArtSlideValidator);

        RuleForEach(dto => dto.Arts)
            .SetValidator(artCreateUpdateDTOValidator);
    }

    private static bool BeValidTeaserLength(string? teaser)
    {
        if (string.IsNullOrEmpty(teaser))
        {
            return true;
        }

        bool containsNewLine = teaser.Contains('\n');
        int maxLength = containsNewLine ? TeaserMaxLengthWithNewLine : TeaserMaxLength;

        return teaser.Length <= maxLength;
    }

    private static bool HaveExactlyOneBlackAndWhite(IEnumerable<ImageDetailsDto> images)
        => images is not null && images.Count(i => i.Alt == $"{(int)ImageAssigment.Blackandwhite}") == 1;

    private static bool HaveAtMostOneAnimation(IEnumerable<ImageDetailsDto> images)
        => images is null || images.Count(i => i.Alt == $"{(int)ImageAssigment.Animation}") <= 1;

    private static bool HaveAtMostOneRelatedFigure(IEnumerable<ImageDetailsDto> images)
        => images is null || images.Count(i => i.Alt == $"{(int)ImageAssigment.Relatedfigure}") <= 1;
}
