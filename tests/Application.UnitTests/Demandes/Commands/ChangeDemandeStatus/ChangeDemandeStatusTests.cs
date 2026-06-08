using IdaraTech.Application.Common.Exceptions;
using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Demandes.Commands.ChangeDemandeStatus;
using IdaraTech.Application.UnitTests.Common;
using IdaraTech.Domain.Constants;
using IdaraTech.Domain.Entities;
using IdaraTech.Domain.Enums;
using IdaraTech.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IdaraTech.Application.UnitTests.Demandes.Commands.ChangeDemandeStatus;

public class ChangeDemandeStatusTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IUser> _userMock = null!;
    private Mock<IIdentityService> _identityServiceMock = null!;
    private ChangeDemandeStatusCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _userMock = new Mock<IUser>();
        _identityServiceMock = new Mock<IIdentityService>();

        _handler = new ChangeDemandeStatusCommandHandler(
            _contextMock.Object,
            _userMock.Object,
            _identityServiceMock.Object);
    }

    [Test]
    public async Task Handle_ShouldChangeStatusAndEmitEvent_WhenUserIsAdmin()
    {
        // Arrange
        var demande = new Demande
        {
            Id = 1,
            Description = "Demande test",
            Statut = Statut.EnAttente,
            ServiceId = 2
        };

        var demandesDbSet = MockDbSetHelper.CreateMockDbSet(new List<Demande> { demande });
        _contextMock.Setup(c => c.Demandes).Returns(demandesDbSet.Object);

        // Setup user as administrator
        _userMock.Setup(u => u.Id).Returns("admin-user-id");
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Administrator });

        var command = new ChangeDemandeStatusCommand
        {
            DemandeId = 1,
            NewStatus = Statut.Traitee
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        demande.Statut.ShouldBe(Statut.Traitee);
        demande.DomainEvents.Count.ShouldBe(1);
        var statusChangedEvent = demande.DomainEvents.First().ShouldBeOfType<DemandeStatusChangedEvent>();
        statusChangedEvent.Demande.ShouldBe(demande);
        statusChangedEvent.PreviousStatus.ShouldBe(Statut.EnAttente);

        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenDemandeDoesNotExist()
    {
        // Arrange
        var demandesDbSet = MockDbSetHelper.CreateMockDbSet(new List<Demande>());
        _contextMock.Setup(c => c.Demandes).Returns(demandesDbSet.Object);

        var command = new ChangeDemandeStatusCommand
        {
            DemandeId = 999,
            NewStatus = Statut.Traitee
        };

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken.None));
    }

    [Test]
    public async Task Handle_ShouldChangeStatus_WhenUserIsAgentInSameService()
    {
        // Arrange
        var serviceId = 3;
        var agentEmail = "agent@idaratech.tn";
        var agentUserId = "agent-user-id";

        var demande = new Demande
        {
            Id = 1,
            Description = "Demande test",
            Statut = Statut.EnAttente,
            ServiceId = serviceId
        };

        var agent = new Agent
        {
            Id = 10,
            Email = agentEmail,
            ServiceId = serviceId
        };

        var demandesDbSet = MockDbSetHelper.CreateMockDbSet(new List<Demande> { demande });
        var agentsDbSet = MockDbSetHelper.CreateMockDbSet(new List<Agent> { agent });

        _contextMock.Setup(c => c.Demandes).Returns(demandesDbSet.Object);
        _contextMock.Setup(c => c.Agents).Returns(agentsDbSet.Object);

        _userMock.Setup(u => u.Id).Returns(agentUserId);
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Agent });

        _identityServiceMock.Setup(i => i.GetUserNameAsync(agentUserId))
            .ReturnsAsync(agentEmail);

        var command = new ChangeDemandeStatusCommand
        {
            DemandeId = 1,
            NewStatus = Statut.Refusee
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        demande.Statut.ShouldBe(Statut.Refusee);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserIsAgentInDifferentService()
    {
        // Arrange
        var agentEmail = "agent@idaratech.tn";
        var agentUserId = "agent-user-id";

        var demande = new Demande
        {
            Id = 1,
            Description = "Demande test",
            Statut = Statut.EnAttente,
            ServiceId = 3 // ServiceId = 3
        };

        var agent = new Agent
        {
            Id = 10,
            Email = agentEmail,
            ServiceId = 4 // ServiceId = 4 (Different!)
        };

        var demandesDbSet = MockDbSetHelper.CreateMockDbSet(new List<Demande> { demande });
        var agentsDbSet = MockDbSetHelper.CreateMockDbSet(new List<Agent> { agent });

        _contextMock.Setup(c => c.Demandes).Returns(demandesDbSet.Object);
        _contextMock.Setup(c => c.Agents).Returns(agentsDbSet.Object);

        _userMock.Setup(u => u.Id).Returns(agentUserId);
        _userMock.Setup(u => u.Roles).Returns(new List<string> { Roles.Agent });

        _identityServiceMock.Setup(i => i.GetUserNameAsync(agentUserId))
            .ReturnsAsync(agentEmail);

        var command = new ChangeDemandeStatusCommand
        {
            DemandeId = 1,
            NewStatus = Statut.Refusee
        };

        // Act & Assert
        await Should.ThrowAsync<ForbiddenAccessException>(async () =>
            await _handler.Handle(command, CancellationToken.None));
    }
}
