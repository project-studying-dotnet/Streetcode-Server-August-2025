using System.Linq.Expressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Tag;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Streetcode.Update;
using Streetcode.BLL.Validators.AdditionalContent.Tag;
using Streetcode.BLL.Validators.Streetcode;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Streetcode;

public class UpdateStreetcodeValidatorTests
{
    private readonly UpdateStreetcodeValidator _validator;
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<BaseStreetcodeValidator> _mockBaseStreetcodeValidator;
    private readonly Mock<TagValidator> _mockTagValidator;

    public UpdateStreetcodeValidatorTests()
    {
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockBaseStreetcodeValidator = new Mock<BaseStreetcodeValidator>();
        _mockTagValidator = new Mock<TagValidator>();

        _validator = new UpdateStreetcodeValidator(
            _mockRepositoryWrapper.Object,
            _mockBaseStreetcodeValidator.Object,
            _mockTagValidator.Object);
    }

    [Fact]
    public async Task ShouldReturnSuccessResult_WhenAllFieldsAreValid()
    {
        // Arrange
        var streetcode = GetValidStreetcodeUpdateDTO();
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
        var streetcode = GetValidStreetcodeUpdateDTO();
        SetupRepositoryWrapper(2);
        var expectedMessage = "Index must be unique.";

        // Act
        var result = await _validator.ValidateAsync(streetcode);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == expectedMessage);
    }

    [Fact]
    public async Task ShouldCallBaseValidator()
    {
        // Arrange
        var streetcode = GetValidStreetcodeUpdateDTO();
        SetupRepositoryWrapperForValidScenario();

        // Act
        await _validator.ValidateAsync(streetcode);

        // Assert
        _mockBaseStreetcodeValidator.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<StreetcodeCreateUpdateDTO>>(), default), Times.Once);
    }

    [Fact]
    public async Task ShouldCallChildValidators()
    {
        // Arrange
        var streetcode = GetValidStreetcodeUpdateDTO();
        SetupRepositoryWrapperForValidScenario();

        // Act
        await _validator.ValidateAsync(streetcode);

        // Assert
        _mockTagValidator.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<CreateTagDTO>>(), default), Times.AtLeast(1));
    }

    private void SetupRepositoryWrapperForValidScenario()
    {
        _mockRepositoryWrapper.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>?>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>?>()))
            .ReturnsAsync(null as StreetcodeContent);
    }

    private void SetupRepositoryWrapper(int id)
    {
        _mockRepositoryWrapper.Setup(repo => repo.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync(new StreetcodeContent { Id = id });
    }

    private static StreetcodeUpdateDTO GetValidStreetcodeUpdateDTO()
    {
        return new StreetcodeUpdateDTO
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
