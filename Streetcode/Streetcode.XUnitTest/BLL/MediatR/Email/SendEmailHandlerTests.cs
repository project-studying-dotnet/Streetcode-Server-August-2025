using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using FluentResults;
using MediatR;
using Moq;
using FluentAssertions;
using Xunit;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.Interfaces.Email;
using Streetcode.BLL.MediatR.Email;
using Streetcode.BLL.DTO.Email;
using Streetcode.DAL.Entities.AdditionalContent.Email;

public class SendEmailHandlerTests
{
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILoggerService> _loggerMock;
    private readonly SendEmailHandler _handler;

    public SendEmailHandlerTests()
    {
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILoggerService>();
        _handler = new SendEmailHandler(_emailServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOk_WhenEmailSentSuccessfully()
    {
        var command = new SendEmailCommand(new EmailDTO
        {
            From = "test@domain.com",
            Content = "Hello!"
        });

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<Message>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Unit.Value, result.Value);
        _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenEmailSendingFails()
    {
        var command = new SendEmailCommand(new EmailDTO
        {
            From = "fail@domain.com",
            Content = "Failure test"
        });

        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<Message>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("Failed to send email message", result.Errors[0].Message);

        _loggerMock.Verify(
            l => l.LogError(command, It.Is<string>(msg => msg.Contains("Failed to send email message"))),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectMessage_ToEmailService()
    {
        var emailDto = new EmailDTO
        {
            From = "sender@test.com",
            Content = "Unit test content"
        };
        var command = new SendEmailCommand(emailDto);

        Message capturedMessage = null!;
        _emailServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<Message>()))
            .Callback<Message>(msg => capturedMessage = msg)
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedMessage);
        Assert.Equal("sender@test.com", capturedMessage.From);
        Assert.Equal("Unit test content", capturedMessage.Content);
        Assert.Contains(capturedMessage.To, addr => addr.Address == "streetcodeua@gmail.com");
    }
}
