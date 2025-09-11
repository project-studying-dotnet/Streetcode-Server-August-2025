using Moq;
using Streetcode.BLL.Interfaces.Text;
using Streetcode.BLL.MediatR.Streetcode.Text.GetParsed;
using Streetcode.BLL.Resources;
using Xunit;

namespace Streetcode.XUnitTest.BLL.MediatR.StreetCode.Text.GetParsed
{
    public class GetParsedTextTests
    {
        private readonly Mock<ITextService> _textServiceMock;
        private readonly GetParsedTextAdminPreviewHandler _handler;
        public GetParsedTextTests()
        {
            _textServiceMock = new Mock<ITextService>();

            _handler = new GetParsedTextAdminPreviewHandler(_textServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenTextIsParsedSeccussfully_ReturnOkResult()
        {
            var inputText = "original text";
            var parsedText = "parsed text";

            _textServiceMock.Setup(s => s.AddTermsTag(inputText))
            .ReturnsAsync(parsedText);

            var command = new GetParsedTextForAdminPreviewCommand(inputText);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(parsedText, result.Value);
            _textServiceMock.Verify(s => s.AddTermsTag(inputText), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenParsingFails_ReturnsFailResult()
        {
            // Arrange
            var inputText = "text to parse";

            _textServiceMock.Setup(s => s.AddTermsTag(inputText))
                .ReturnsAsync((string?)null);

            var command = new GetParsedTextForAdminPreviewCommand(inputText);
            var errorMsg = Errors_Common.FailedToParseText;

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains(result.Errors, e => e.Message.Contains(errorMsg));
            _textServiceMock.Verify(s => s.AddTermsTag(inputText), Times.Once);
        }
    }
}
