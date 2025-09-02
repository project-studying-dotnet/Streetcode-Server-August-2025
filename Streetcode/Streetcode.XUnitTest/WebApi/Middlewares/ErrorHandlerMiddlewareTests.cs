using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using Streetcode.WebApi.Middlewares;
using Xunit;
using System.Text.Json.Serialization;

namespace Streetcode.XUnitTest.WebApi.Middlewares;

public class ErrorHandlerMiddlewareTests
{
    private readonly Mock<ILogger<ErrorHandlerMiddleware>> _loggerMock;

    public ErrorHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ErrorHandlerMiddleware>>();
    }

    private DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream(); // щоб читати результат
        return context;
    }

    private static string GetResponseBody(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(response.Body);
        return reader.ReadToEnd();
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNext()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = new ErrorHandlerMiddleware(_ => Task.CompletedTask, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithException_ReturnsInternalServerError()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = new ErrorHandlerMiddleware(_ => throw new Exception("Test error"), _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);

        var body = GetResponseBody(context.Response);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(body);

        Assert.NotNull(errorResponse);
        Assert.Equal("Test error", errorResponse.Error);
        Assert.Equal((int)HttpStatusCode.InternalServerError, errorResponse.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithKeyNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var context = CreateHttpContext();
        var middleware = new ErrorHandlerMiddleware(_ => throw new KeyNotFoundException("Not found"), _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, context.Response.StatusCode);

        var body = GetResponseBody(context.Response);
        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(body);

        Assert.NotNull(errorResponse);
        Assert.Equal("Not found", errorResponse.Error);
        Assert.Equal((int)HttpStatusCode.NotFound, errorResponse.StatusCode);
    }

    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }
    }
}