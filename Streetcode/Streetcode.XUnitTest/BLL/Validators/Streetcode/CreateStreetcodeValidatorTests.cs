using System.Linq.Expressions;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Streetcode.Create;
using Streetcode.BLL.Validators.AdditionalContent.Tag;
using Streetcode.BLL.Validators.Streetcode;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Streetcode;

public class CreateStreetcodeValidatorTests
{
    private readonly CreateStreetcodeValidator _validator;
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<BaseStreetcodeValidator> _mockBaseStreetcodeValidator;
    private readonly Mock<TagValidator> _mockTagValidator;

    public CreateStreetcodeValidatorTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockBaseStreetcodeValidator = new Mock<BaseStreetcodeValidator>();
        _mockTagValidator = new Mock<TagValidator>();

        _validator = new CreateStreetcodeValidator(
            _mockRepositoryWrapper.Object,
            _mockBaseStreetcodeValidator.Object,
            _mockTagValidator.Object);
    }

    [Fact]
    public async Task ShouldReturnSuccessResult_WhenAllFieldsAreValid()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        SetupRepositoryWrapperForValidScenario();

        // Act
        var result = await _validator.ValidateAsync(streetcode);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ShouldReturnError_WhenIndexIsNotUnique()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        SetupRepositoryWrapper(1);
        var expectedMessage = "Index must be unique.";

        // Act
        var result = await _validator.TestValidateAsync(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.Index)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public async Task ShouldReturnError_WhenImagesDetailsIsEmpty()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        streetcode.ImagesDetails = new List<ImageDetailsDto>();
        SetupRepositoryWrapperForValidScenario();
        var expectedMessage = "At least one image detail is required.";

        // Act
        var result = await _validator.TestValidateAsync(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor(sc => sc.ImagesDetails)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public async Task ShouldReturnError_WhenImageDoesNotExist()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        SetupRepositoryWrapperForValidScenario();
        streetcode.ImagesDetails.First().ImageId = 99;
        _mockRepositoryWrapper
            .Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Image, bool>>>(),
                It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()))
            .ReturnsAsync(null as Image);

        var expectedMessage = "One or more images do not exist.";

        // Act
        var result = await _validator.TestValidateAsync(streetcode);

        // Assert
        result.ShouldHaveValidationErrorFor($"Streetcode.ImagesDetails.ImageId[{0}]")
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public async Task ShouldCallBaseValidator()
    {
        // Arrange
        var streetcode = GetValidStreetcodeDto();
        SetupRepositoryWrapperForValidScenario();

        // Act
        var result = await _validator.ValidateAsync(streetcode);

        // Assert
        _mockBaseStreetcodeValidator.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<StreetcodeCreateUpdateDTO>>(), default), Times.Once);
    }

    private void SetupRepositoryWrapperForValidScenario()
    {
        _mockRepositoryWrapper
            .Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>?>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>?>()))
            .ReturnsAsync(null as StreetcodeContent);

        _mockRepositoryWrapper
            .Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Image, bool>>>(),
                It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()))
            .ReturnsAsync(new Image { Id = 22 });
    }

    private void SetupRepositoryWrapper(int id)
    {
        _mockRepositoryWrapper
            .Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync(new StreetcodeContent { Id = id });

        _mockRepositoryWrapper
            .Setup(repo => repo.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Image, bool>>>(),
                It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()))
            .ReturnsAsync(new Image { Id = id });
    }

    private static StreetcodeCreateDTO GetValidStreetcodeDto()
    {
        return new StreetcodeCreateDTO
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
            Tags =
            [
                new (),
            ],
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
