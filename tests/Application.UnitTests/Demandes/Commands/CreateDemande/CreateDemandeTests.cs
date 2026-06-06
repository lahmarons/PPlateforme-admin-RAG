using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Demandes.Commands.CreateDemande;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IdaraTech.Application.UnitTests.Demandes.Commands.CreateDemande;

public class CreateDemandeTests
{
    private Mock<IApplicationDbContext> _contextMock;
    private Mock<DbSet<Demande>> _demandesSetMock;
    private CreateDemandeCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _demandesSetMock = new Mock<DbSet<Demande>>();

        _contextMock.Setup(c => c.Demandes).Returns(_demandesSetMock.Object);
        _handler = new CreateDemandeCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ShouldCreateDemandeWithInitialStatusAndDomainEvent()
    {
        // Arrange
        var command = new CreateDemandeCommand
        {
            CitoyenId = 1,
            Description = "Demande d'acte de naissance",
            ServiceId = 3,
            Documents = new List<DocumentUploadDto>()
        };

        Demande? savedDemande = null;
        _demandesSetMock.Setup(s => s.Add(It.IsAny<Demande>()))
            .Callback<Demande>(d => savedDemande = d);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _demandesSetMock.Verify(s => s.Add(It.IsAny<Demande>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        savedDemande.ShouldNotBeNull();
        savedDemande.CitoyenId.ShouldBe(command.CitoyenId);
        savedDemande.Description.ShouldBe(command.Description);
        savedDemande.ServiceId.ShouldBe(command.ServiceId);
        savedDemande.Statut.ShouldBe(Statut.EnAttente);
        savedDemande.Documents.ShouldBeEmpty();

        // Verify Domain Event
        savedDemande.DomainEvents.Count.ShouldBe(1);
        var createdEvent = savedDemande.DomainEvents.First().ShouldBeOfType<DemandeCreatedEvent>();
        createdEvent.Demande.ShouldBe(savedDemande);
    }

    [Test]
    public async Task Handle_ShouldDecodeBase64DocumentsAndAddThemToDemande()
    {
        // Arrange
        var rawContent = "Fichier de test contenu";
        var bytes = Encoding.UTF8.GetBytes(rawContent);
        var base64Content = Convert.ToBase64String(bytes);

        var command = new CreateDemandeCommand
        {
            CitoyenId = 2,
            Description = "Demande de CIN",
            ServiceId = null,
            Documents = new List<DocumentUploadDto>
            {
                new("cin_copie.pdf", "PDF", base64Content),
                new("photos.jpg", "JPEG", base64Content)
            }
        };

        Demande? savedDemande = null;
        _demandesSetMock.Setup(s => s.Add(It.IsAny<Demande>()))
            .Callback<Demande>(d => savedDemande = d);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        savedDemande.ShouldNotBeNull();
        savedDemande.Documents.Count.ShouldBe(2);

        var firstDoc = savedDemande.Documents[0];
        firstDoc.Nom.ShouldBe("cin_copie.pdf");
        firstDoc.Type.ShouldBe("PDF");
        firstDoc.Contenu.ShouldBe(bytes);

        var secondDoc = savedDemande.Documents[1];
        secondDoc.Nom.ShouldBe("photos.jpg");
        secondDoc.Type.ShouldBe("JPEG");
        secondDoc.Contenu.ShouldBe(bytes);
    }
}
