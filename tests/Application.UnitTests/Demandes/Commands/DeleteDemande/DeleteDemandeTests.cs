using IdaraTech.Application.Common.Exceptions;
using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Demandes.Commands.DeleteDemande;
using IdaraTech.Application.UnitTests.Common;
using IdaraTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdaraTech.Application.UnitTests.Demandes.Commands.DeleteDemande;

public class DeleteDemandeTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<DbSet<Demande>>? _demandesSetMock;
    private DeleteDemandeCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeleteDemandeCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ShouldDeleteDemande_WhenDemandeExists()
    {
        // Arrange
        var demande = new Demande
        {
            Id = 42,
            Description = "Demande to delete"
        };

        var list = new List<Demande> { demande };
        var dbSetMock = MockDbSetHelper.CreateMockDbSet(list);
        _demandesSetMock = dbSetMock;

        _contextMock.Setup(c => c.Demandes).Returns(_demandesSetMock.Object);

        var command = new DeleteDemandeCommand(42);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _demandesSetMock!.Verify(s => s.Remove(It.IsAny<Demande>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenDemandeDoesNotExist()
    {
        // Arrange
        var list = new List<Demande>();
        var dbSetMock = MockDbSetHelper.CreateMockDbSet(list);
        _demandesSetMock = dbSetMock;

        _contextMock.Setup(c => c.Demandes).Returns(_demandesSetMock.Object);

        var command = new DeleteDemandeCommand(99);

        // Act & Assert
        await Should.ThrowAsync<Ardalis.GuardClauses.NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken.None));
    }
}
