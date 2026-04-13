using IdaraTech.Application.Common.Behaviours;
using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Demandes.Commands.CreateDemande;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace IdaraTech.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<CreateDemandeCommand>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<CreateDemandeCommand>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<CreateDemandeCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new CreateDemandeCommand { CitoyenId = 1, Description = "title" }, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<CreateDemandeCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new CreateDemandeCommand { CitoyenId = 1, Description = "title" }, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}
