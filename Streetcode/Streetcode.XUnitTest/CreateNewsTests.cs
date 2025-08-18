using AutoMapper;
using FluentAssertions;
using FluentResults;
using Moq;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.Create;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Repositories.Interfaces.Newss;
using Xunit;

namespace Streetcode.XUnitTest.BLL_Tests.MediatR.News;

public class CreateNewsTests
{
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
    private readonly Mock<INewsRepository> _mockNewsRepository;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly CreateNewsHandler _handler;

    public CreateNewsTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
        _mockNewsRepository = new Mock<INewsRepository>();
        _mockLogger = new Mock<ILoggerService>();

        _mockRepositoryWrapper.Setup(repo => repo.NewsRepository)
            .Returns(_mockNewsRepository.Object);

        _handler = new CreateNewsHandler(
            _mockMapper.Object,
            _mockRepositoryWrapper.Object,
            _mockLogger.Object);
    }
}