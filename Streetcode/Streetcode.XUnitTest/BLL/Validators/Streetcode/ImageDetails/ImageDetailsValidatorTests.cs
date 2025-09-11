using System.Linq.Expressions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Resources;
using Streetcode.BLL.Util.Extensions;
using Streetcode.BLL.Validators.Streetcode.ImageDetails;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Streetcode.ImageDetails;

public class ImageDetailsValidatorTests
{
    private readonly ImageDetailsValidator _validator;
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;

    public ImageDetailsValidatorTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _validator = new ImageDetailsValidator(_mockRepositoryWrapper.Object);
    }

    [Fact]
    public async Task ShouldReturnSuccessResult_WhenAllFieldsAreValid()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        SetupRepositoryForValidScenario();

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ShouldReturnError_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        imageDetails.Title = new string('A', ImageDetailsValidator.TitleMaxLength + 1);
        SetupRepositoryForValidScenario();
        var expectedMessage = Errors_Validation.MaxLength.FormatWith("Title", ImageDetailsValidator.TitleMaxLength);

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public async Task ShouldReturnSuccess_WhenTitleIsAtMaxLength()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        imageDetails.Title = new string('A', ImageDetailsValidator.TitleMaxLength);
        SetupRepositoryForValidScenario();

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task ShouldReturnSuccess_WhenTitleIsNull()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        imageDetails.Title = null;
        SetupRepositoryForValidScenario();

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task ShouldReturnSuccess_WhenTitleIsEmpty()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        imageDetails.Title = string.Empty;
        SetupRepositoryForValidScenario();

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task ShouldReturnError_WhenAltExceedsMaxLength()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        imageDetails.Alt = new string('B', ImageDetailsValidator.AltMaxLength + 1);
        SetupRepositoryForValidScenario();
        var expectedMessage = Errors_Validation.MaxLength.FormatWith("Alt", ImageDetailsValidator.AltMaxLength);

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Alt)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public async Task ShouldReturnSuccess_WhenAltIsAtMaxLength()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        imageDetails.Alt = new string('B', ImageDetailsValidator.AltMaxLength);
        SetupRepositoryForValidScenario();

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Alt);
    }

    [Fact]
    public async Task ShouldReturnSuccess_WhenAltIsNull()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        imageDetails.Alt = null;
        SetupRepositoryForValidScenario();

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Alt);
    }

    [Fact]
    public async Task ShouldReturnSuccess_WhenAltIsEmpty()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        imageDetails.Alt = string.Empty;
        SetupRepositoryForValidScenario();

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Alt);
    }

    [Fact]
    public async Task ShouldReturnError_WhenImageIdDoesNotExist()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        SetupRepositoryForNonExistentImage();
        SetupRepositoryForNoExistingImageDetails();
        var expectedMessage = Errors_Validation.ImageDoesntExist.FormatWith(imageDetails.ImageId);

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageId)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public async Task ShouldReturnSuccess_WhenImageIdExists()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        SetupRepositoryForExistingImage(imageDetails.ImageId);
        SetupRepositoryForNoExistingImageDetails();

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ImageId);
    }

    [Fact]
    public async Task ShouldReturnError_WhenImageDetailsWithSameImageIdAlreadyExists()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        imageDetails.ImageId = 5;
        SetupRepositoryForExistingImage(imageDetails.ImageId);
        SetupRepositoryForExistingImageDetails(imageDetails.ImageId, differentId: 999);
        var expectedMessage = Errors_Validation.MustBeUnique.FormatWith("ImageId");

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage(expectedMessage);
    }

    [Fact]
    public async Task ShouldReturnSuccess_WhenImageDetailsWithSameImageIdIsSameEntity()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        imageDetails.Id = 10;
        imageDetails.ImageId = 5;
        SetupRepositoryForExistingImage(imageDetails.ImageId);
        SetupRepositoryForExistingImageDetails(imageDetails.ImageId, imageDetails.Id);

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public async Task ShouldReturnSuccess_WhenNoExistingImageDetailsForImageId()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        SetupRepositoryForExistingImage(imageDetails.ImageId);
        SetupRepositoryForNoExistingImageDetails();

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public async Task ShouldNotValidateUniqueness_WhenImageIdIsZero()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        imageDetails.ImageId = 0;
        SetupRepositoryForNonExistentImage();

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        // Should have error for non-existent image but not for uniqueness
        result.ShouldHaveValidationErrorFor(x => x.ImageId)
            .WithErrorMessage(Errors_Validation.ImageDoesntExist.FormatWith(imageDetails.ImageId));

        // Verify that ImageDetails repository was not called for uniqueness check
        _mockRepositoryWrapper.Verify(
            r => r.ImageDetailsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Media.Images.ImageDetails, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Media.Images.ImageDetails>, IIncludableQueryable<DAL.Entities.Media.Images.ImageDetails, object>>>()),
            Times.Never);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task ShouldReturnError_WhenImageIdIsNegative(int negativeImageId)
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        imageDetails.ImageId = negativeImageId;
        SetupRepositoryForNonExistentImage();
        SetupRepositoryForNoExistingImageDetails();

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageId)
            .WithErrorMessage(Errors_Validation.ImageDoesntExist.FormatWith(imageDetails.ImageId));
    }

    [Fact]
    public async Task ShouldReturnMultipleErrors_WhenMultipleValidationsFail()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        imageDetails.Title = new string('A', ImageDetailsValidator.TitleMaxLength + 1);
        imageDetails.Alt = new string('B', ImageDetailsValidator.AltMaxLength + 1);
        imageDetails.ImageId = 999;
        SetupRepositoryForNonExistentImage();
        SetupRepositoryForExistingImageDetails(999, differentId: 888);

        // Act
        var result = await _validator.TestValidateAsync(imageDetails);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Alt);
        result.ShouldHaveValidationErrorFor(x => x.ImageId);
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public async Task ShouldCallRepositoryOnce_WhenValidating()
    {
        // Arrange
        var imageDetails = GetValidImageDetailsDto();
        SetupRepositoryForValidScenario();

        // Act
        await _validator.TestValidateAsync(imageDetails);

        // Assert
        _mockRepositoryWrapper.Verify(
            r => r.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Image, bool>>>(),
                It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()),
            Times.Once);

        _mockRepositoryWrapper.Verify(
            r => r.ImageDetailsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Media.Images.ImageDetails, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Media.Images.ImageDetails>, IIncludableQueryable<DAL.Entities.Media.Images.ImageDetails, object>>>()),
            Times.Once);
    }

    private static ImageDetailsDto GetValidImageDetailsDto()
    {
        return new ImageDetailsDto
        {
            Id = 1,
            ImageId = 5,
            Title = "Valid Title",
            Alt = "Valid Alt Text"
        };
    }

    private void SetupRepositoryForValidScenario()
    {
        SetupRepositoryForExistingImage(5);
        SetupRepositoryForNoExistingImageDetails();
    }

    private void SetupRepositoryForExistingImage(int imageId)
    {
        _mockRepositoryWrapper.Setup(r => r.ImageRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<Image, bool>>>(expr => true),
                It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()))
            .ReturnsAsync(new Image { Id = imageId });
    }

    private void SetupRepositoryForNonExistentImage()
    {
        _mockRepositoryWrapper.Setup(r => r.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Image, bool>>>(),
                It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()))
            .ReturnsAsync((Image?)null);
    }

    private void SetupRepositoryForNoExistingImageDetails()
    {
        _mockRepositoryWrapper.Setup(r => r.ImageDetailsRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<DAL.Entities.Media.Images.ImageDetails, bool>>>(),
                It.IsAny<Func<IQueryable<DAL.Entities.Media.Images.ImageDetails>, IIncludableQueryable<DAL.Entities.Media.Images.ImageDetails, object>>>()))
            .ReturnsAsync((DAL.Entities.Media.Images.ImageDetails?)null);
    }

    private void SetupRepositoryForExistingImageDetails(int imageId, int differentId)
    {
        _mockRepositoryWrapper.Setup(r => r.ImageDetailsRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<DAL.Entities.Media.Images.ImageDetails, bool>>>(expr => true),
                It.IsAny<Func<IQueryable<DAL.Entities.Media.Images.ImageDetails>, IIncludableQueryable<DAL.Entities.Media.Images.ImageDetails, object>>>()))
            .ReturnsAsync(new DAL.Entities.Media.Images.ImageDetails
            {
                Id = differentId,
                ImageId = imageId
            });
    }
}