using Streetcode.BLL.Validators.Helpers;
using Xunit;

namespace Streetcode.XUnitTest.BLL.Validators.Helpers
{
    public class ValidationHelperTests
    {
        [Theory]
        [InlineData("https://www.google.com")]
        [InlineData("http://example.com/path?query=123")]
        [InlineData("ftp://ftp.example.com")]
        public void BeValidUrl_Should_Return_True_For_Valid_Urls(string url)
        {
            // Act
            var result = ValidationHelper.BeValidUrl(url);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("not a url")]
        [InlineData("www.missing-scheme.com")]
        [InlineData("")]
        [InlineData(null)]
        public void BeValidUrl_Should_Return_False_For_Invalid_Urls(string url)
        {
            // Act
            var result = ValidationHelper.BeValidUrl(url);

            // Assert
            Assert.False(result);
        }
    }
}