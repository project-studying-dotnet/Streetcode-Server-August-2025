using FluentValidation.TestHelper;
using Moq;
using Streetcode.BLL.DTO.ArtGallery;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.ArtGallery;
using Streetcode.BLL.Validators.Media.Image.Art;
using Streetcode.BLL.Validators.Streetcode;
using Streetcode.BLL.Validators.Streetcode.ImageDetails;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Streetcode;

public class BaseStreetcodeValidatorsTests
{
    private readonly BaseStreetcodeValidator _validator;
    private readonly Mock<StreetcodeArtSlideValidator> _mockStreetcodeArtSlideValidator;
    private readonly Mock<ArtCreateUpdateDTOValidator> _mockArtCreateUpdateDTOValidator;
    private readonly Mock<ImageDetailsValidator> _mockImageDetailsValidator;

    public BaseStreetcodeValidatorsTests()
    {
        _mockStreetcodeArtSlideValidator = new Mock<StreetcodeArtSlideValidator>();
        _mockArtCreateUpdateDTOValidator = new Mock<ArtCreateUpdateDTOValidator>();
        _mockImageDetailsValidator = new Mock<ImageDetailsValidator>(Mock.Of<IRepositoryWrapper>());

        _validator = new BaseStreetcodeValidator(
            _mockStreetcodeArtSlideValidator.Object,
            _mockArtCreateUpdateDTOValidator.Object,
            _mockImageDetailsValidator.Object);
    }

    [Fact]
    public void ShouldReturnSuccessResult_WhenAllFieldsAreValid()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();

        // Act
        var result = _validator.Validate(streetcode);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10000)]
    public void ShouldReturnError_WhenIndexIsOutOfRange(int index)
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.Index = index;
        var expectedMessage = Errors_Validation.MustBeBetween.FormatWith("Index", BaseStreetcodeValidator.IndexMinValue, BaseStreetcodeValidator.IndexMaxValue);

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.Index)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenIndexIsNull()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.Index = 0; // This will be treated as null/invalid
        var expectedMessage = Errors_Validation.MustBeBetween.FormatWith("Index", BaseStreetcodeValidator.IndexMinValue, BaseStreetcodeValidator.IndexMaxValue);

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.Index)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenFirstNameIsTooLong()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.FirstName = new string('A', BaseStreetcodeValidator.FirstNameMaxLength + 1);
        var expectedMessage = Errors_Validation.MaxLength.FormatWith("FirstName", BaseStreetcodeValidator.FirstNameMaxLength);

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.FirstName)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenLastNameIsTooLong()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.LastName = new string('A', BaseStreetcodeValidator.LastNameMaxLength + 1);
        var expectedMessage = Errors_Validation.MaxLength.FormatWith("LastName", BaseStreetcodeValidator.LastNameMaxLength);

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.LastName)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenTitleIsEmpty()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.Title = string.Empty;
        var expectedMessage = Errors_Validation.CannotBeEmpty.FormatWith("Title");

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.Title)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenTitleIsTooLong()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.Title = new string('A', BaseStreetcodeValidator.TitleMaxLength + 1);
        var expectedMessage = Errors_Validation.MaxLength.FormatWith("Title", BaseStreetcodeValidator.TitleMaxLength);

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.Title)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenAliasIsTooLong()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.Alias = new string('A', BaseStreetcodeValidator.AliasMaxLength + 1);
        var expectedMessage = Errors_Validation.MaxLength.FormatWith("Alias", BaseStreetcodeValidator.AliasMaxLength);

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.Alias)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenTransliterationUrlIsEmpty()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.TransliterationUrl = string.Empty;
        var expectedMessage = Errors_Validation.CannotBeEmpty.FormatWith("TransliterationUrl");

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.TransliterationUrl)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenTransliterationUrlIsTooLong()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.TransliterationUrl = new string('a', BaseStreetcodeValidator.TransliterationUrlMaxLength + 1);
        var expectedMessage = Errors_Validation.MaxLength.FormatWith("TransliterationUrl", BaseStreetcodeValidator.TransliterationUrlMaxLength);

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.TransliterationUrl)
            .WithErrorMessage(expectedMessage);
    }

    [Theory]
    [InlineData("invalid url!")]
    [InlineData("invalid_url")]
    [InlineData("invalid.url")]
    [InlineData("Invalid-URL!")]
    [InlineData("http://www.invalid.url")]
    [InlineData("тест.юа")]
    public void ShouldReturnError_WhenTransliterationUrlIsInvalid(string url)
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.TransliterationUrl = url;
        var expectedMessage = Errors_Validation.TransliterationUrlFormat;

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.TransliterationUrl)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenDateStringIsEmpty()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.DateString = string.Empty;
        var expectedMessage = Errors_Validation.CannotBeEmpty.FormatWith("DateString");

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.DateString)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenDateStringIsTooLong()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.DateString = new string('A', BaseStreetcodeValidator.DateStringMaxLength + 1);
        var expectedMessage = Errors_Validation.MaxLength.FormatWith("DateString", BaseStreetcodeValidator.DateStringMaxLength);

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.DateString)
            .WithErrorMessage(expectedMessage);
    }

    [Theory]
    [InlineData("27 August 1856 - 28 May 1916")]
    [InlineData("27 серпня 1856 року - 28 травня 1916 року!")]
    [InlineData("september 2025 - december 2025")]
    [InlineData("2025/2026")]
    [InlineData("tests#$%^@%&")]
    public void ShouldReturnError_WhenDateStringIsInvalid(string dateString)
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.DateString = dateString;
        var expectedMessage = Errors_Validation.DateStringFormat;

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.DateString)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenTeaserIsEmpty()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.Teaser = string.Empty;
        var expectedMessage = "Teaser is required.";

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.Teaser)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenTeaserIsTooLong_WithoutNewline()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.Teaser = new string('A', BaseStreetcodeValidator.TeaserMaxLength + 1);
        var expectedMessage = $"Teaser cannot exceed {BaseStreetcodeValidator.TeaserMaxLength} characters, or {BaseStreetcodeValidator.TeaserMaxLengthWithNewLine} characters if it contains a newline.";

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.Teaser)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenTeaserIsTooLong_WithNewline()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.Teaser = new string('A', BaseStreetcodeValidator.TeaserMaxLengthWithNewLine) + "\nB";
        var expectedMessage = $"Teaser cannot exceed {BaseStreetcodeValidator.TeaserMaxLength} characters, or {BaseStreetcodeValidator.TeaserMaxLengthWithNewLine} characters if it contains a newline.";

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.Teaser)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnSuccess_WhenTeaserIsValidLength_WithNewline()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.Teaser = new string('A', BaseStreetcodeValidator.TeaserMaxLengthWithNewLine - 1) + "\n";

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldNotHaveValidationErrorFor(sc => sc.Teaser);
    }

    [Fact]
    public void ShouldReturnError_WhenStreetcodeTypeIsInvalid()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.StreetcodeType = (StreetcodeType)999;
        var expectedMessage = Errors_Validation.Invalid.FormatWith("StreetcodeType");

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.StreetcodeType)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenStatusIsInvalid()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.Status = (StreetcodeStatus)999;
        var expectedMessage = Errors_Validation.Invalid.FormatWith("StreetcodeStatus");

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.Status)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenEventStreetcodeHasNotEmpty_FirstNameAndLastName()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.StreetcodeType = StreetcodeType.Event;
        var expectedMessage = Errors_Validation.EventStreetcodeCannotHasFirstName;

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnSuccess_WhenEventStreetcodeHasEmpty_FirstNameAndLastName()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.StreetcodeType = StreetcodeType.Event;
        streetcode.FirstName = null;
        streetcode.LastName = null;

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldNotHaveValidationErrorFor(sc => sc);
    }

    [Fact]
    public void ShouldReturnError_WhenNotExactlyOneBlackAndWhiteImage()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.ImagesDetails = new List<ImageDetailsDto>();
        var expectedMessage = Errors_Validation.MustContainExactlyOneBlackAndWhiteImage;

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.ImagesDetails)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenThereAreTwoAnimationImages()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.ImagesDetails =
        [
            new()
            {
                Alt = "1", // Black and white
            },
            new()
            {
                Alt = "0", // Animation
            },
            new()
            {
                Alt = "0", // Animation (duplicate)
            },
        ];

        var expectedMessage = Errors_Validation.MustContainAtMostOneColoredImage;

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.ImagesDetails)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenThereAreTwoRelatedFigureImages()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.ImagesDetails =
        [
            new()
            {
                Alt = "1", // Black and white
            },
            new()
            {
                Alt = "2", // Related figure
            },
            new()
            {
                Alt = "2", // Related figure (duplicate)
            },
        ];
        var expectedMessage = Errors_Validation.MustContainAtMostOneRelatedFigureImage;

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.ImagesDetails)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnSuccess_WhenValidImageAssignments()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.ImagesDetails =
        [
            new()
            {
                Alt = "1", // Black and white (required)
                ImageId = 1,
                Title = "Black and white"
            },
            new()
            {
                Alt = "0", // Animation (optional, max 1)
                ImageId = 2,
                Title = "Animation"
            },
            new()
            {
                Alt = "2", // Related figure (optional, max 1)
                ImageId = 3,
                Title = "Related figure"
            },
        ];

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldNotHaveValidationErrorFor(sc => sc.ImagesDetails);
    }

    private static StreetcodeCreateUpdateDTO GetValidStreetcodeDto()
    {
        return new StreetcodeCreateUpdateDTO
        {
            Index = 1,
            FirstName = "Ivan",
            LastName = "Franko",
            Alias = "kameniar",
            Title = "Ivan Franko",
            Teaser = "Видатний український письменник, поет, вчений і громадський діяч.",
            TransliterationUrl = "ivan-franko",
            DateString = "27 серпня (9 вересня) 1856 року – 28 травня (10 червня) 1916 року",
            StreetcodeType = StreetcodeType.Person,
            Status = StreetcodeStatus.Published,
            ImagesDetails =
            [
                new ()
                {
                    Id = 2,
                    ImageId = 5,
                    Title = "Franko_black&white",
                    Alt = "1", // Black and white image assignment
                },
            ],
            StreetcodeArtSlides = new List<StreetcodeArtSlideCreateUpdateDTO>(),
            Arts = new List<ArtCreateUpdateDTO>()
        };
    }
}
