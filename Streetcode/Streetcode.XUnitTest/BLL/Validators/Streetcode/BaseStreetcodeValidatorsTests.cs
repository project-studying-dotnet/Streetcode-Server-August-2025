using FluentValidation.TestHelper;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Validators.Streetcode;
using Streetcode.DAL.Enums;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Streetcode;

public class BaseStreetcodeValidatorsTests
{
    private readonly BaseStreetcodeValidator _validator;

    public BaseStreetcodeValidatorsTests()
    {
        _validator = new BaseStreetcodeValidator();
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
        var expectedMessage = $"Index must be between {BaseStreetcodeValidator.IndexMinValue} and {BaseStreetcodeValidator.IndexMaxValue}.";

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
        var expectedMessage = $"First name cannot exceed {BaseStreetcodeValidator.FirstNameMaxLength} characters.";

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
        var expectedMessage = $"Last name cannot exceed {BaseStreetcodeValidator.LastNameMaxLength} characters.";

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
        var expectedMessage = "Title is required.";

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
        var expectedMessage = $"Title cannot exceed {BaseStreetcodeValidator.TitleMaxLength} characters.";

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
        var expectedMessage = $"Alias cannot exceed {BaseStreetcodeValidator.AliasMaxLength} characters.";

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
        var expectedMessage = "Transliteration URL is required.";

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
        streetcode.TransliterationUrl = new string('A', BaseStreetcodeValidator.TransliterationUrlMaxLength + 1);
        var expectedMessage = $"Transliteration URL cannot exceed {BaseStreetcodeValidator.TransliterationUrlMaxLength} characters.";

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
        var expectedMessage = "Transliteration URL can only contain lowercase letters, numbers, and hyphens.";

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
        var expectedMessage = "Date string is required.";

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
        var expectedMessage = $"Date string cannot exceed {BaseStreetcodeValidator.DateStringMaxLength} characters.";

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.DateString)
            .WithErrorMessage(expectedMessage);
    }

    [Theory]
    [InlineData("27 August 1856 - 28 May 1916")]
    [InlineData("27 серпня 1856 року - 28 травня 1916 року!")]
    [InlineData("semptember 2025 - december 2025")]
    [InlineData("2025/2026")]
    [InlineData("tests#$%^@%&")]
    public void ShouldReturnError_WhenDateStringIsInvalid(string dateString)
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.DateString = dateString;
        var expectedMessage = "Date string can only contain numbers, Ukrainian letters, spaces, parentheses, and hyphens.";

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
    public void ShouldReturnError_WhenStreetcodeTypeIsInvalid()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.StreetcodeType = (StreetcodeType)999;
        var expectedMessage = "Invalid streetcode type.";

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
        var expectedMessage = "Invalid streetcode status.";

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
        var expectedMessage = "First name and Last name must be empty for Event streetcode type.";

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenNotExactlyOneBlackAndWhiteImage()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.ImagesDetails = new List<ImageDetailsDto>();
        var expectedMessage = "There must be exactly one black and white image.";

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.ImagesDetails)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public void ShouldReturnError_WhenThereAreTwoColoredImages()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.ImagesDetails =
        [
            new()
            {
                Alt = "1",
            },
            new()
            {
                Alt = "0",
            },
            new()
            {
                Alt = "0",
            },
        ];

        var expectedMessage = "There can be at most one animation image.";

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
                Alt = "1",
            },
            new()
            {
                Alt = "2",
            },
            new()
            {
                Alt = "2",
            },
        ];
        var expectedMessage = "There can be at most one related figure image.";

        // Act
        var result = _validator.TestValidate(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.ImagesDetails)
            .WithErrorMessage(expectedMessage);
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
                    Alt = "1",
                },
            ]
        };
    }
}
