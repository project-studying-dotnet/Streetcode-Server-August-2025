using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Entities.AdditionalContent.Coordinates.Types;
using Streetcode.DAL.Entities.Feedback;
using Streetcode.DAL.Entities.Media;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Entities.Sources;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Entities.Streetcode.Types;
using Streetcode.DAL.Entities.Team;
using Streetcode.DAL.Entities.Timeline;
using Streetcode.DAL.Entities.Transactions;
using Streetcode.DAL.Entities.Users;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Persistence;
using Streetcode.WebApi.Extensions;
using Xunit;

namespace Streetcode.XUnitTest.WebApi.Extensions;

public class SeedingLocalExtensionTests
{
    private readonly ServiceCollection _services;
    private readonly ServiceProvider _serviceProvider;
    private readonly StreetcodeDbContext _dbContext;
    private readonly Mock<IBlobService> _blobServiceMock;

    public SeedingLocalExtensionTests()
    {
        // Create real service collection and configure it
        _services = new ServiceCollection();

        // Add in-memory database
        _services.AddDbContext<StreetcodeDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Add logging
        _services.AddLogging();

        // Mock blob service
        _blobServiceMock = new Mock<IBlobService>();
        var blobServiceFactoryMock = new Mock<IBlobServiceFactory>();
        blobServiceFactoryMock.Setup(x => x.CreateBlobService()).Returns(_blobServiceMock.Object);
        _services.AddSingleton(blobServiceFactoryMock.Object);

        // Mock blob options
        var blobOptions = new BlobEnvironmentVariables
        {
            StorageType = "local",
            BlobStorePath = Path.GetTempPath()
        };
        _services.Configure<BlobEnvironmentVariables>(opts =>
        {
            opts.StorageType = blobOptions.StorageType;
            opts.BlobStorePath = blobOptions.BlobStorePath;
        });

        // Add Identity services
        _services.AddIdentity<User, IdentityRole<int>>()
            .AddEntityFrameworkStores<StreetcodeDbContext>()
            .AddUserManager<UserManager<User>>()
            .AddSignInManager<SignInManager<User>>();

        // Build service provider
        _serviceProvider = _services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<StreetcodeDbContext>();

        // Ensure database is created
        _dbContext.Database.EnsureCreated();

        // Create test JSON files
        CreateTestJsonFiles();
    }

    private void CreateTestJsonFiles()
    {
        var testImages = new List<Image>
        {
            new Image { Id = 1, BlobName = "test1.jpg", MimeType = "image/jpeg", Base64 = "base64data1" },
            new Image { Id = 25, BlobName = "test25.jpg", MimeType = "image/jpeg", Base64 = "base64data25" },
            new Image { Id = 26, BlobName = "test26.jpg", MimeType = "image/jpeg", Base64 = "base64data26" }
        };

        var testAudios = new List<Audio>
        {
            new Audio { Id = 1, BlobName = "test1.mp3", MimeType = "audio/mpeg", Base64 = "audiobase64data1" },
            new Audio { Id = 2, BlobName = "test2.mp3", MimeType = "audio/mpeg", Base64 = "audiobase64data2" }
        };

        var imageJson = JsonConvert.SerializeObject(testImages);
        var audioJson = JsonConvert.SerializeObject(testAudios);

        // Create directories and files for testing
        var imageDir = Path.GetDirectoryName("../Streetcode.DAL/InitialData/images.json");
        var audioDir = Path.GetDirectoryName("../Streetcode.DAL/InitialData/audios.json");

        if (!string.IsNullOrEmpty(imageDir))
        {
            Directory.CreateDirectory(imageDir);
            File.WriteAllText("../Streetcode.DAL/InitialData/images.json", imageJson);
        }

        if (!string.IsNullOrEmpty(audioDir))
        {
            Directory.CreateDirectory(audioDir);
            File.WriteAllText("../Streetcode.DAL/InitialData/audios.json", audioJson);
        }
    }

    [Fact]
    public void AddIdentityServices_ConfiguresIdentityServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<StreetcodeDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Act
        var result = services.AddIdentityServices();

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetService<UserManager<User>>());
        Assert.NotNull(serviceProvider.GetService<SignInManager<User>>());
    }

    [Fact]
    public async Task SeedDataAsync_WhenImagesExist_DoesNotSeedAdditionalData()
    {
        // Arrange
        _dbContext.Images.Add(new Image { Id = 999, BlobName = "existing.jpg", MimeType = "image/jpeg" });
        await _dbContext.SaveChangesAsync();

        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        Assert.Single(_dbContext.Images);
        Assert.Equal("existing.jpg", _dbContext.Images.First().BlobName);
    }

    [Fact]
    public async Task SeedDataAsync_WhenImagesEmpty_ReadsImageJsonFile()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        Assert.True(_dbContext.Images.Count() >= 3); // At least the test images we created
        Assert.Contains(_dbContext.Images, i => i.BlobName == "test1.jpg");
        Assert.Contains(_dbContext.Images, i => i.BlobName == "test25.jpg");
    }

    [Fact]
    public async Task SeedDataAsync_WhenImagesEmpty_ReadsAudioJsonFile()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        Assert.True(_dbContext.Audios.Count() >= 2);
        Assert.Contains(_dbContext.Audios, a => a.BlobName == "test1.mp3");
        Assert.Contains(_dbContext.Audios, a => a.BlobName == "test2.mp3");
    }

    [Fact]
    public async Task SeedDataAsync_WhenImagesEmpty_DeserializesImageJson()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        var images = _dbContext.Images.ToList();
        Assert.True(images.Count >= 3);
        Assert.All(images, img => Assert.NotNull(img.BlobName));
        Assert.All(images, img => Assert.NotNull(img.MimeType));
    }

    [Fact]
    public async Task SeedDataAsync_WhenImagesEmpty_DeserializesAudioJson()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        var audios = _dbContext.Audios.ToList();
        Assert.True(audios.Count >= 2);
        Assert.All(audios, audio => Assert.NotNull(audio.BlobName));
        Assert.All(audios, audio => Assert.NotNull(audio.MimeType));
    }

    [Fact]
    public async Task SeedDataAsync_WhenImagesEmpty_CallsSeedMediaFilesForImages()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        // Verify that SaveFileInStorageWithName was called for images
        _blobServiceMock.Verify(x => x.SaveFileInStorageWithName(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SeedDataAsync_WhenImagesEmpty_CallsSeedMediaFilesForAudios()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        // Verify blob service was called for audio files too
        _blobServiceMock.Verify(x => x.SaveFileInStorageWithName(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.AtLeast(2)); // At least 2 calls for audios + images
    }

    [Fact]
    public async Task SeedDataAsync_WhenImagesEmpty_AddsImagesToDbContext()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        Assert.True(_dbContext.Images.Count() >= 3);
        var addedImages = _dbContext.Images.ToList();
        Assert.Contains(addedImages, i => i.Id == 1);
        Assert.Contains(addedImages, i => i.Id == 25);
        Assert.Contains(addedImages, i => i.Id == 26);
    }

    [Fact]
    public async Task SeedDataAsync_WhenImagesEmpty_CallsSaveChangesAsync()
    {
        // Arrange
        var app = CreateMockWebApplication();
        var initialChangeCount = await _dbContext.SaveChangesAsync();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        // Verify data was actually saved to database
        Assert.True(_dbContext.Images.Count() > 0);
        Assert.True(_dbContext.Audios.Count() > 0);
    }

    [Fact]
    public async Task SeedDataAsync_WhenResponsesEmpty_SeedsResponses()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        Assert.Equal(2, _dbContext.Responses.Count());
        var responses = _dbContext.Responses.ToList();
        Assert.Contains(responses, r => r.Name == "Alex" && r.Description == "Good Job");
        Assert.Contains(responses, r => r.Name == "Danyil" && r.Description == "Nice project");
    }

    [Fact]
    public async Task SeedDataAsync_WhenTermsEmpty_SeedsTerms()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        Assert.Equal(4, _dbContext.Terms.Count());
        var terms = _dbContext.Terms.ToList();
        Assert.Contains(terms, t => t.Title == "етнограф");
        Assert.Contains(terms, t => t.Title == "гравер");
        Assert.Contains(terms, t => t.Title == "кріпак");
        Assert.Contains(terms, t => t.Title == "мачуха");
    }

    [Fact]
    public async Task SeedDataAsync_WhenRelatedTermsEmpty_SeedsRelatedTerms()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        Assert.Single(_dbContext.RelatedTerms);
        var relatedTerm = _dbContext.RelatedTerms.First();
        Assert.Equal("кріпаків", relatedTerm.Word);
        Assert.Equal(3, relatedTerm.TermId);
    }

    [Fact]
    public async Task SeedDataAsync_WhenTeamMembersEmpty_SeedsTeamMembers()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        Assert.Equal(3, _dbContext.TeamMembers.Count());
        var teamMembers = _dbContext.TeamMembers.ToList();
        Assert.Contains(teamMembers, tm => tm.FirstName == "Inna" && tm.LastName == "Krupnyk");
        Assert.Contains(teamMembers, tm => tm.FirstName == "Danyil" && tm.LastName == "Terentiev");
        Assert.Contains(teamMembers, tm => tm.FirstName == "Nadia" && tm.LastName == "Kischchuk");
    }

    [Fact]
    public async Task SeedDataAsync_WhenPositionsEmpty_SeedsPositions()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        Assert.Single(_dbContext.Positions);
        var position = _dbContext.Positions.First();
        Assert.Equal("Голова і засновниця ГО", position.Position);
    }

    [Fact]
    public async Task SeedDataAsync_WhenTeamMemberLinksEmpty_SeedsTeamMemberLinks()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        Assert.Equal(12, _dbContext.TeamMemberLinks.Count());
        var links = _dbContext.TeamMemberLinks.ToList();
        Assert.Contains(links, l => l.LogoType == LogoType.YouTube);
        Assert.Contains(links, l => l.LogoType == LogoType.Facebook);
        Assert.Contains(links, l => l.LogoType == LogoType.Instagram);
        Assert.Contains(links, l => l.LogoType == LogoType.Twitter);
    }

    [Fact]
    public async Task SeedDataAsync_WhenNewsEmpty_SeedsNews()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        Assert.Equal(3, _dbContext.News.Count());
        var news = _dbContext.News.ToList();
        Assert.Contains(news, n => n.Title == "27 квітня встановлюємо перший стріткод!");
        Assert.Contains(news, n => n.URL == "first-streetcode");
        Assert.Contains(news, n => n.URL == "danya");
        Assert.Contains(news, n => n.URL == "scum");
    }

    [Fact]
    public async Task SeedDataAsync_WhenStreetcodesEmpty_SeedsStreetcodes()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        Assert.Equal(2, _dbContext.Streetcodes.Count());
        var streetcodes = _dbContext.Streetcodes.ToList();
        Assert.Contains(streetcodes, s => s.TransliterationUrl == "taras-shevchenko");
        Assert.Contains(streetcodes, s => s.TransliterationUrl == "roman-ratushnyi");
    }

    [Fact]
    public async Task SeedDataAsync_WhenAllEntitiesEmpty_SeedsAllEntities()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert - Test that all major entity types are seeded
        Assert.True(_dbContext.Images.Any());
        Assert.True(_dbContext.Audios.Any());
        Assert.True(_dbContext.Responses.Any());
        Assert.True(_dbContext.Terms.Any());
        Assert.True(_dbContext.TeamMembers.Any());
        Assert.True(_dbContext.News.Any());
        Assert.True(_dbContext.Streetcodes.Any());
        Assert.True(_dbContext.Videos.Any());
        Assert.True(_dbContext.Partners.Any());
        Assert.True(_dbContext.Arts.Any());
        Assert.True(_dbContext.Texts.Any());
        Assert.True(_dbContext.TimelineItems.Any());
        Assert.True(_dbContext.TransactionLinks.Any());
        Assert.True(_dbContext.Facts.Any());
        Assert.True(_dbContext.Tags.Any());
    }

    [Fact]
    public async Task SeedDataAsync_CreatesUsersWithCorrectProperties()
    {
        // Arrange
        var app = CreateMockWebApplication();

        // Act
        await SeedingLocalExtension.SeedDataAsync(app);

        // Assert
        var testUsers = _dbContext.Users.Where(u => u.Email.Contains("testuser")).ToList();
        Assert.Equal(2, testUsers.Count);

        var testUser1 = testUsers.FirstOrDefault(u => u.Email == "testuser@gmail.com");
        Assert.NotNull(testUser1);
        Assert.Equal("Test", testUser1.Name);
        Assert.Equal("User", testUser1.Surname);
        Assert.Equal(UserRole.User, testUser1.Role);
        Assert.True(testUser1.EmailConfirmed);

        var adminUser = _dbContext.Users.FirstOrDefault(u => u.Email == "adminuser@gmail.com");
        Assert.NotNull(adminUser);
        Assert.Equal(UserRole.Administrator, adminUser.Role);
    }

    private WebApplication CreateMockWebApplication()
    {
        var builder = WebApplication.CreateBuilder();

        // Configure services in the builder
        builder.Services.AddDbContext<StreetcodeDbContext>(options =>
            options.UseInMemoryDatabase(_dbContext.Database.GetDbConnection().Database));

        builder.Services.AddSingleton(_serviceProvider.GetRequiredService<IBlobServiceFactory>());
        builder.Services.Configure<BlobEnvironmentVariables>(opts =>
        {
            opts.StorageType = "local";
            opts.BlobStorePath = Path.GetTempPath();
        });

        builder.Services.AddIdentity<User, IdentityRole<int>>()
            .AddEntityFrameworkStores<StreetcodeDbContext>()
            .AddUserManager<UserManager<User>>()
            .AddSignInManager<SignInManager<User>>();

        // Override the DbContext service with our test instance
        builder.Services.AddSingleton(_dbContext);

        return builder.Build();
    }
}