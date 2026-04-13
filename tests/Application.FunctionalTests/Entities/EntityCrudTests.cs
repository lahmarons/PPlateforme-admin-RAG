using IdaraTech.Application.Admins.Commands.CreateAdmin;
using IdaraTech.Application.Admins.Commands.DeleteAdmin;
using IdaraTech.Application.Admins.Commands.UpdateAdmin;
using IdaraTech.Application.Admins.Queries.GetAdminsList;
using IdaraTech.Application.Agents.Commands.CreateAgent;
using IdaraTech.Application.Agents.Commands.DeleteAgent;
using IdaraTech.Application.Agents.Commands.UpdateAgent;
using IdaraTech.Application.Agents.Queries.GetAgentsList;
using IdaraTech.Application.Citoyens.Commands.CreateCitoyen;
using IdaraTech.Application.Citoyens.Commands.DeleteCitoyen;
using IdaraTech.Application.Citoyens.Commands.UpdateCitoyen;
using IdaraTech.Application.Citoyens.Queries.GetCitoyens;
using IdaraTech.Application.Demandes.Commands.CreateDemande;
using IdaraTech.Application.Demandes.Commands.DeleteDemande;
using IdaraTech.Application.Demandes.Commands.UpdateDemande;
using IdaraTech.Application.Demandes.Queries.GetDemandById;
using IdaraTech.Application.Documents.Commands.CreateDocument;
using IdaraTech.Application.Documents.Commands.DeleteDocument;
using IdaraTech.Application.Documents.Commands.UpdateDocument;
using IdaraTech.Application.Documents.Queries.GetDocumentsList;
using IdaraTech.Application.Services.Commands.CreateService;
using IdaraTech.Application.Services.Commands.DeleteService;
using IdaraTech.Application.Services.Commands.UpdateService;
using IdaraTech.Application.Services.Queries.GetServicesList;
using IdaraTech.Domain.Entities;
using IdaraTech.Domain.Enums;

namespace IdaraTech.Application.FunctionalTests.Entities;

using static Testing;

public class EntityCrudTests : BaseTestFixture
{
    [Test]
    public async Task ShouldCreateAndQueryAllEntities()
    {
        var citoyenId = await SendAsync(new CreateCitoyenCommand
        {
            Nom = "Citoyen One",
            Email = "citoyen1@test.local",
            MotDePasse = "Pass123!",
            Adresse = "Address One",
            Telephone = "000111222"
        });

        var serviceId = await SendAsync(new CreateServiceCommand
        {
            Nom = "Service One",
            Description = "Description One"
        });

        var agentId = await SendAsync(new CreateAgentCommand
        {
            Nom = "Agent One",
            Email = "agent1@test.local",
            MotDePasse = "Pass123!",
            Matricule = "MAT-001",
            Grade = "A",
            ServiceId = serviceId
        });

        var adminId = await SendAsync(new CreateAdminCommand
        {
            Nom = "Admin One",
            Email = "admin1@test.local",
            MotDePasse = "Pass123!"
        });

        var demandeId = await SendAsync(new CreateDemandeCommand
        {
            CitoyenId = citoyenId,
            Description = "Demande One",
            ServiceId = serviceId
        });

        var documentId = await SendAsync(new CreateDocumentCommand
        {
            Nom = "Doc One",
            Type = "pdf",
            DemandeId = demandeId
        });

        var citoyens = await SendAsync(new GetCitoyensQuery());
        var services = await SendAsync(new GetServicesQuery());
        var agents = await SendAsync(new GetAgentsQuery());
        var admins = await SendAsync(new GetAdminsQuery());
        var documents = await SendAsync(new GetDocumentsQuery());
        var demande = await SendAsync(new GetDemandeByIdQuery { Id = demandeId });

        citoyens.Any(c => c.Id == citoyenId).ShouldBeTrue();
        services.Any(s => s.Id == serviceId).ShouldBeTrue();
        agents.Any(a => a.Id == agentId).ShouldBeTrue();
        admins.Any(a => a.Id == adminId).ShouldBeTrue();
        documents.Any(d => d.Id == documentId).ShouldBeTrue();
        demande.Id.ShouldBe(demandeId);
    }

    [Test]
    public async Task ShouldUpdateAndDeleteAllEntities()
    {
        var citoyenId = await SendAsync(new CreateCitoyenCommand
        {
            Nom = "Citoyen Two",
            Email = "citoyen2@test.local",
            MotDePasse = "Pass123!",
            Adresse = "Address Two",
            Telephone = "333444555"
        });

        var serviceId = await SendAsync(new CreateServiceCommand
        {
            Nom = "Service Two",
            Description = "Description Two"
        });

        var agentId = await SendAsync(new CreateAgentCommand
        {
            Nom = "Agent Two",
            Email = "agent2@test.local",
            MotDePasse = "Pass123!",
            Matricule = "MAT-002",
            Grade = "B",
            ServiceId = serviceId
        });

        var adminId = await SendAsync(new CreateAdminCommand
        {
            Nom = "Admin Two",
            Email = "admin2@test.local",
            MotDePasse = "Pass123!"
        });

        var demandeId = await SendAsync(new CreateDemandeCommand
        {
            CitoyenId = citoyenId,
            Description = "Demande Two",
            ServiceId = serviceId
        });

        var documentId = await SendAsync(new CreateDocumentCommand
        {
            Nom = "Doc Two",
            Type = "jpg",
            DemandeId = demandeId
        });

        await SendAsync(new UpdateCitoyenCommand
        {
            Id = citoyenId,
            Nom = "Citoyen Two Updated",
            Email = "citoyen2.updated@test.local",
            MotDePasse = "Pass123!",
            Adresse = "Address Two Updated",
            Telephone = "666777888"
        });

        await SendAsync(new UpdateServiceCommand
        {
            Id = serviceId,
            Nom = "Service Two Updated",
            Description = "Description Two Updated"
        });

        await SendAsync(new UpdateAgentCommand
        {
            Id = agentId,
            Nom = "Agent Two Updated",
            Email = "agent2.updated@test.local",
            MotDePasse = "Pass123!",
            Matricule = "MAT-002-U",
            Grade = "A",
            ServiceId = serviceId
        });

        await SendAsync(new UpdateAdminCommand
        {
            Id = adminId,
            Nom = "Admin Two Updated",
            Email = "admin2.updated@test.local",
            MotDePasse = "Pass123!"
        });

        await SendAsync(new UpdateDemandeCommand
        {
            Id = demandeId,
            Description = "Demande Two Updated",
            ServiceId = serviceId,
            AgentId = agentId,
            Statut = Statut.EnCours
        });

        await SendAsync(new UpdateDocumentCommand
        {
            Id = documentId,
            Nom = "Doc Two Updated",
            Type = "png",
            DemandeId = demandeId
        });

        var citoyen = await FindAsync<Citoyen>(citoyenId);
        var service = await FindAsync<Service>(serviceId);
        var agent = await FindAsync<Agent>(agentId);
        var admin = await FindAsync<Admin>(adminId);
        var demande = await FindAsync<Demande>(demandeId);
        var document = await FindAsync<Document>(documentId);

        citoyen.ShouldNotBeNull();
        service.ShouldNotBeNull();
        agent.ShouldNotBeNull();
        admin.ShouldNotBeNull();
        demande.ShouldNotBeNull();
        document.ShouldNotBeNull();

        citoyen!.Nom.ShouldBe("Citoyen Two Updated");
        service!.Nom.ShouldBe("Service Two Updated");
        agent!.Matricule.ShouldBe("MAT-002-U");
        admin!.Nom.ShouldBe("Admin Two Updated");
        demande!.Statut.ShouldBe(Statut.EnCours);
        document!.Type.ShouldBe("png");

        await SendAsync(new DeleteDocumentCommand(documentId));
        await SendAsync(new DeleteDemandeCommand(demandeId));
        await SendAsync(new DeleteAdminCommand(adminId));
        await SendAsync(new DeleteAgentCommand(agentId));
        await SendAsync(new DeleteServiceCommand(serviceId));
        await SendAsync(new DeleteCitoyenCommand(citoyenId));

        (await FindAsync<Document>(documentId)).ShouldBeNull();
        (await FindAsync<Demande>(demandeId)).ShouldBeNull();
        (await FindAsync<Admin>(adminId)).ShouldBeNull();
        (await FindAsync<Agent>(agentId)).ShouldBeNull();
        (await FindAsync<Service>(serviceId)).ShouldBeNull();
        (await FindAsync<Citoyen>(citoyenId)).ShouldBeNull();
    }
}
