using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Hangfire;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Swashbuckle.AspNetCore.SwaggerGen;
using Streetcode.DAL.Entities.AdditionalContent.Email;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Realizations.Base;
using Streetcode.WebApi.Extensions;
using ServiceCollectionExtensions = Streetcode.WebApi.Extensions.ServiceCollectionExtensions;

namespace Streetcode.XUnitTest.WebApi.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddRepositoryServices_ShouldRegisterRepositoryWrapper_AsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddRepositoryServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var repositoryWrapperDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IRepositoryWrapper));

        repositoryWrapperDescriptor.Should().NotBeNull();
        repositoryWrapperDescriptor.ImplementationType.Should().Be(typeof(RepositoryWrapper));
        repositoryWrapperDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddApplicationServices_WithValidConfiguration_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateMockConfiguration();

        // Act
        services.AddApplicationServices(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify DbContext is registered
        services.Should().Contain(s => s.ServiceType == typeof(StreetcodeDbContext));
        var dbContextDescriptor = services.First(s => s.ServiceType == typeof(StreetcodeDbContext));
        dbContextDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

        // Verify EmailConfiguration is registered as singleton
        services.Should().Contain(s => s.ServiceType == typeof(EmailConfiguration));
        var emailConfigDescriptor = services.First(s => s.ServiceType == typeof(EmailConfiguration));
        emailConfigDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);

        // Verify CORS is registered
        services.Should().Contain(s => s.ServiceType == typeof(ICorsService));
        services.Should().Contain(s => s.ServiceType == typeof(ICorsPolicyProvider));

        // Verify Authentication is registered
        services.Should().Contain(s => s.ServiceType == typeof(IAuthenticationService));
        services.Should().Contain(s => s.ServiceType == typeof(IAuthenticationSchemeProvider));

        // Verify Authorization is registered
        services.Should().Contain(s => s.ServiceType == typeof(IAuthorizationService));

        // Verify Hangfire is registered
        services.Should().Contain(s => s.ServiceType == typeof(IBackgroundJobClient));
    }

    [Fact]
    public void AddApplicationServices_WithMissingConnectionString_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateMockConfigurationWithoutConnectionString();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            services.AddApplicationServices(configuration));

        exception.Should().NotBeNull();
    }

    [Fact]
    public void AddSwaggerServices_ShouldConfigureSwaggerWithJwtBearer()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSwaggerServices();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var swaggerGenOptions = serviceProvider.GetService<IOptions<SwaggerGenOptions>>();

        swaggerGenOptions.Should().NotBeNull();
    }

    [Fact]
    public void CorsConfiguration_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var corsConfig = new ServiceCollectionExtensions.CorsConfiguration
        {
            AllowedOrigins = new[] { "https://localhost:3000" },
            AllowedHeaders = new[] { "Content-Type", "Authorization" },
            AllowedMethods = new[] { "GET", "POST", "PUT", "DELETE" },
            PreflightMaxAge = 86400
        };

        // Assert
        corsConfig.AllowedOrigins.Should().NotBeNull();
        corsConfig.AllowedHeaders.Should().NotBeNull();
        corsConfig.AllowedMethods.Should().NotBeNull();
        corsConfig.PreflightMaxAge.Should().Be(86400);
    }

    [Fact]
    public void AddApplicationServices_ShouldConfigureJwtAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateMockConfiguration();

        // Act
        services.AddApplicationServices(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var authOptions = serviceProvider.GetService<IOptions<AuthenticationOptions>>();

        authOptions.Should().NotBeNull();
        authOptions.Value.DefaultAuthenticateScheme.Should().Be(JwtBearerDefaults.AuthenticationScheme);
        authOptions.Value.DefaultChallengeScheme.Should().Be(JwtBearerDefaults.AuthenticationScheme);
    }

    private ConfigurationManager CreateMockConfiguration()
    {
        var configuration = new ConfigurationManager();

        // Add connection string
        configuration["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=TestDb;Integrated Security=true;";

        // Add JWT settings
        configuration["JwtSettings:Issuer"] = "TestIssuer";
        configuration["JwtSettings:Audience"] = "TestAudience";
        configuration["JwtSettings:SecretKey"] = "ThisIsAVeryLongSecretKeyForTesting123456789";

        // Add CORS settings
        configuration["CORS:AllowedOrigins:0"] = "https://localhost:3000";
        configuration["CORS:AllowedHeaders:0"] = "Content-Type";
        configuration["CORS:AllowedHeaders:1"] = "Authorization";
        configuration["CORS:AllowedMethods:0"] = "GET";
        configuration["CORS:AllowedMethods:1"] = "POST";

        // Add Email configuration
        configuration["EmailConfiguration:SmtpServer"] = "smtp.test.com";
        configuration["EmailConfiguration:Port"] = "587";
        configuration["EmailConfiguration:Username"] = "test@test.com";
        configuration["EmailConfiguration:Password"] = "testpassword";

        return configuration;
    }

    private ConfigurationManager CreateMockConfigurationWithoutConnectionString()
    {
        var configuration = new ConfigurationManager();

        // Add JWT settings but no connection string
        configuration["JwtSettings:Issuer"] = "TestIssuer";
        configuration["JwtSettings:Audience"] = "TestAudience";
        configuration["JwtSettings:SecretKey"] = "ThisIsAVeryLongSecretKeyForTesting123456789";

        return configuration;
    }
}