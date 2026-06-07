using IdaraTech.Domain.Constants;
using IdaraTech.Domain.Entities;
using IdaraTech.Domain.Enums;
using IdaraTech.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdaraTech.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private const string AdministratorEmail = "administrator@localhost";
    private const string AdministratorPassword = "Administrator1!";
    private const string AgentEmail = "agent@localhost";
    private const string AgentPassword = "Agent123!";

    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            // Skip EnsureDeletedAsync in development to preserve existing data
            // await _context.Database.EnsureDeletedAsync();
            if (await _context.Database.CanConnectAsync())
            {
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
                if (!appliedMigrations.Any())
                {
                    var databaseCreator = _context.Database.GetService<IRelationalDatabaseCreator>();
                    if (databaseCreator.Exists())
                    {
                        await _context.Database.EnsureDeletedAsync();
                    }
                }
            }

            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        var roles = new[]
        {
            Roles.Citoyen,
            Roles.Agent,
            Roles.Administrator
        };

        foreach (var roleName in roles)
        {
            if (await _roleManager.FindByNameAsync(roleName) is null)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        await SeedIdentityUserAsync(AdministratorEmail, AdministratorPassword, Roles.Administrator);
        await SeedIdentityUserAsync(AgentEmail, AgentPassword, Roles.Agent);

        await SeedDomainAdminAsync();
        await SeedDomainAgentAsync();
        await SeedPrestatairesEtServicesAsync();
        await SeedAdditionalTestDataAsync();
    }

    private async Task SeedIdentityUserAsync(string email, string password, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
            }
        }

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }
    }

    private async Task SeedDomainAdminAsync()
    {
        if (!_context.Admins.Any(admin => admin.Email == AdministratorEmail))
        {
            _context.Admins.Add(new Admin
            {
                Nom = "Administrateur",
                Email = AdministratorEmail,
                MotDePasse = AdministratorPassword
            });

            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedDomainAgentAsync()
    {
        if (!_context.Agents.Any(agent => agent.Email == AgentEmail))
        {
            _context.Agents.Add(new Agent
            {
                Nom = "Agent",
                Email = AgentEmail,
                MotDePasse = AgentPassword,
                Matricule = "AG-001",
                Grade = "Senior",
                IsActive = true
            });

            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedPrestatairesEtServicesAsync()
    {
        // Define Prestataires we want to ensure exist
        var prestatairesData = new[]
        {
            new { Key = "MI", Nom = "Ministère de l'Intérieur", Description = "Responsable de la sécurité nationale, de l'état civil et de la délivrance des documents d'identité." },
            new { Key = "MT", Nom = "Ministère du Transport (ATTT)", Description = "Agence Technique des Transports Terrestres - gestion des permis de conduire." },
            new { Key = "CNSS", Nom = "Caisse Nationale de Sécurité Sociale (CNSS)", Description = "Organisme de sécurité sociale et de retraite pour le secteur privé." },
            new { Key = "CNAM", Nom = "Caisse Nationale d'Assurance Maladie (CNAM)", Description = "Remboursement des soins et gestion de l'assurance maladie." },
            new { Key = "MUN", Nom = "Municipalités et Communes (État Civil)", Description = "Gestion de l'état civil (actes de naissance, mariage) et légalisation." },
            new { Key = "ANETI", Nom = "Agence Nationale pour l'Emploi (ANETI)", Description = "Accompagnement des demandeurs d'emploi et gestion des programmes CIVP et Karama." }
        };

        var dbPrestataires = await _context.Prestataires.ToListAsync();
        var keyToPrestataireMap = new Dictionary<string, Prestataire>();

        foreach (var pInfo in prestatairesData)
        {
            var p = dbPrestataires.FirstOrDefault(dbP => dbP.Nom.Equals(pInfo.Nom, StringComparison.OrdinalIgnoreCase));
            if (p == null)
            {
                p = new Prestataire { Nom = pInfo.Nom, Description = pInfo.Description };
                _context.Prestataires.Add(p);
                await _context.SaveChangesAsync(); // save to generate Id
            }
            keyToPrestataireMap[pInfo.Key] = p;
        }

        // Define Services we want to ensure exist
        var servicesData = new[]
        {
            new { Nom = "Carte d'Identité Nationale (CIN)", Description = "Première demande ou renouvellement de la carte d'identité.", PrestataireKey = "MI" },
            new { Nom = "Passeport Tunisien", Description = "Délivrance et renouvellement du passeport ordinaire ou en cas de perte.", PrestataireKey = "MI" },
            new { Nom = "Certificat de Résidence", Description = "Justificatif de domicile officiel délivré par le chef de quartier (Omda).", PrestataireKey = "MI" },
            new { Nom = "Permis de Conduire", Description = "Obtention et renouvellement du permis de conduire par l'ATTT.", PrestataireKey = "MT" },
            new { Nom = "Affiliation CNSS", Description = "Affiliation des salariés et des travailleurs indépendants à la sécurité sociale.", PrestataireKey = "CNSS" },
            new { Nom = "Pension de Retraite (CNSS)", Description = "Demande de liquidation de retraite pour les assurés du secteur privé.", PrestataireKey = "CNSS" },
            new { Nom = "Affiliation et Carte CNAM", Description = "Inscription à l'assurance maladie obligatoire et obtention/renouvellement de la carte.", PrestataireKey = "CNAM" },
            new { Nom = "Extrait d'Acte de Naissance", Description = "Délivrance d'extraits de naissance avec ou sans filiation.", PrestataireKey = "MUN" },
            new { Nom = "Célébration de Mariage Civil", Description = "Enregistrement du dossier et célébration du mariage à la mairie.", PrestataireKey = "MUN" },
            new { Nom = "Légalisation de Signature", Description = "Légalisation de signature et certification conforme de documents.", PrestataireKey = "MUN" },
            new { Nom = "Inscription Demandeur d'Emploi (ANETI)", Description = "Inscription sur la plateforme emploi.nat.tn pour l'aide au placement.", PrestataireKey = "ANETI" },
            new { Nom = "Contrat CIVP", Description = "Enregistrement et gestion de contrat d'initiation à la vie professionnelle.", PrestataireKey = "ANETI" },
            new { Nom = "Contrat Al Karama", Description = "Dispositif d'insertion des diplômés du supérieur chômeurs de longue durée.", PrestataireKey = "ANETI" }
        };

        var dbServices = await _context.Services.ToListAsync();
        var newlyAddedServices = new List<Service>();

        foreach (var sInfo in servicesData)
        {
            var s = dbServices.FirstOrDefault(dbS => dbS.Nom.Equals(sInfo.Nom, StringComparison.OrdinalIgnoreCase));
            if (s == null)
            {
                var prestataire = keyToPrestataireMap[sInfo.PrestataireKey];
                s = new Service
                {
                    Nom = sInfo.Nom,
                    Description = sInfo.Description,
                    PrestataireId = prestataire.Id
                };
                _context.Services.Add(s);
                newlyAddedServices.Add(s);
            }
        }

        if (newlyAddedServices.Any())
        {
            await _context.SaveChangesAsync();
        }

        // Link the seeded agent to the CIN service if not already linked
        var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Email == AgentEmail);
        if (agent != null && (agent.ServiceId == null || agent.ServiceId == 0))
        {
            var cinService = await _context.Services.FirstOrDefaultAsync(s => s.Nom.Contains("CIN"));
            if (cinService != null)
            {
                agent.ServiceId = cinService.Id;
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task SeedAdditionalTestDataAsync()
    {
        // 1. Seed Specialty Agents
        var agentsToSeed = new[]
        {
            new { Email = "agent.cin@localhost", Nom = "Mohamed Hédi", Matricule = "AG-CIN-01", Grade = "Junior", ServiceName = "Carte d'Identité Nationale (CIN)" },
            new { Email = "agent.permis@localhost", Nom = "Yassine Ayari", Matricule = "AG-PERM-02", Grade = "Senior", ServiceName = "Permis de Conduire" },
            new { Email = "agent.etatcivil@localhost", Nom = "Fatma Ben Rachid", Matricule = "AG-EC-03", Grade = "Principal", ServiceName = "Extrait d'Acte de Naissance" },
            new { Email = "agent.aneti@localhost", Nom = "Sami Sellami", Matricule = "AG-ANETI-04", Grade = "Senior", ServiceName = "Contrat CIVP" },
            new { Email = "agent.social@localhost", Nom = "Amel Gharbi", Matricule = "AG-SOC-05", Grade = "Junior", ServiceName = "Affiliation CNSS" },
            new { Email = "agent.passeport@localhost", Nom = "Rania Toumi", Matricule = "AG-PASS-06", Grade = "Senior", ServiceName = "Passeport Tunisien" },
            new { Email = "agent.cnam@localhost", Nom = "Ali Belhaj", Matricule = "AG-CNAM-07", Grade = "Junior", ServiceName = "Affiliation et Carte CNAM" },
            new { Email = "agent.commune@localhost", Nom = "Mariem Selmi", Matricule = "AG-MUN-08", Grade = "Senior", ServiceName = "Célébration de Mariage Civil" },
            new { Email = "agent.karama@localhost", Nom = "Slimen Ben Amor", Matricule = "AG-ANETI-09", Grade = "Senior", ServiceName = "Contrat Al Karama" },
            new { Email = "agent.transport@localhost", Nom = "Khaled Najar", Matricule = "AG-ATTT-10", Grade = "Senior", ServiceName = "Permis de Conduire" }
        };

        foreach (var agentData in agentsToSeed)
        {
            // Identity User
            await SeedIdentityUserAsync(agentData.Email, "Agent123!", Roles.Agent);

            // Domain Agent
            if (!_context.Agents.Any(a => a.Email == agentData.Email))
            {
                var service = await _context.Services.FirstOrDefaultAsync(s => s.Nom.Contains(agentData.ServiceName));
                _context.Agents.Add(new Agent
                {
                    Nom = agentData.Nom,
                    Email = agentData.Email,
                    MotDePasse = "Agent123!",
                    Matricule = agentData.Matricule,
                    Grade = agentData.Grade,
                    IsActive = true,
                    ServiceId = service?.Id
                });
            }
        }
        await _context.SaveChangesAsync();

        // 2. Seed Citizens
        var citizensToSeed = new[]
        {
            new { Email = "citoyen@localhost", Nom = "Ahmed Ben Ali", Adresse = "Tunis, Tunisie", Telephone = "+216 22 333 444" },
            new { Email = "citoyen2@localhost", Nom = "Sofia Trabelsi", Adresse = "Sfax, Tunisie", Telephone = "+216 55 666 777" },
            new { Email = "citoyen3@localhost", Nom = "Kamel Mansour", Adresse = "Sousse, Tunisie", Telephone = "+216 99 888 777" },
            new { Email = "citoyen4@localhost", Nom = "Leila Baccouche", Adresse = "Tunis, Tunisie", Telephone = "+216 20 111 222" },
            new { Email = "citoyen5@localhost", Nom = "Oussema Dridi", Adresse = "Bizerte, Tunisie", Telephone = "+216 24 555 666" },
            new { Email = "citoyen6@localhost", Nom = "Rym Chaabane", Adresse = "Monastir, Tunisie", Telephone = "+216 98 444 333" }
        };

        foreach (var citizenData in citizensToSeed)
        {
            // Identity User
            await SeedIdentityUserAsync(citizenData.Email, "Citoyen123!", Roles.Citoyen);

            // Domain Citoyen
            if (!_context.Citoyens.Any(c => c.Email == citizenData.Email))
            {
                _context.Citoyens.Add(new Citoyen
                {
                    Nom = citizenData.Nom,
                    Email = citizenData.Email,
                    MotDePasse = "Citoyen123!",
                    Adresse = citizenData.Adresse,
                    Telephone = citizenData.Telephone
                });
            }
        }
        await _context.SaveChangesAsync();

        // 3. Seed Requests (Demandes) for Ahmed, Sofia, Kamel
        var ahmed = await _context.Citoyens.FirstOrDefaultAsync(c => c.Email == "citoyen@localhost");
        if (ahmed != null && !_context.Demandes.Any(d => d.CitoyenId == ahmed.Id))
        {
            var sofia = await _context.Citoyens.FirstOrDefaultAsync(c => c.Email == "citoyen2@localhost");
            var kamel = await _context.Citoyens.FirstOrDefaultAsync(c => c.Email == "citoyen3@localhost");

            var agentCin = await _context.Agents.FirstOrDefaultAsync(a => a.Email == "agent.cin@localhost");
            var agentPermis = await _context.Agents.FirstOrDefaultAsync(a => a.Email == "agent.permis@localhost");
            var agentEC = await _context.Agents.FirstOrDefaultAsync(a => a.Email == "agent.etatcivil@localhost");
            var agentAneti = await _context.Agents.FirstOrDefaultAsync(a => a.Email == "agent.aneti@localhost");
            var agentSocial = await _context.Agents.FirstOrDefaultAsync(a => a.Email == "agent.social@localhost");

            var serviceCin = await _context.Services.FirstOrDefaultAsync(s => s.Nom.Contains("CIN"));
            var servicePermis = await _context.Services.FirstOrDefaultAsync(s => s.Nom.Contains("Permis"));
            var serviceEC = await _context.Services.FirstOrDefaultAsync(s => s.Nom.Contains("Acte de Naissance"));
            var serviceCivp = await _context.Services.FirstOrDefaultAsync(s => s.Nom.Contains("CIVP"));
            var serviceCnss = await _context.Services.FirstOrDefaultAsync(s => s.Nom.Contains("Affiliation CNSS"));

            // Request 1: Ahmed CIN (EnCours)
            if (ahmed != null && serviceCin != null)
            {
                var d1 = new Demande
                {
                    Description = "Demande de renouvellement de ma Carte d'Identité Nationale suite à un changement d'adresse.",
                    Statut = Statut.EnCours,
                    CitoyenId = ahmed.Id,
                    ServiceId = serviceCin.Id,
                    AgentId = agentCin?.Id
                };
                _context.Demandes.Add(d1);
                await _context.SaveChangesAsync();

                _context.Documents.Add(new Document { Nom = "Ancienne_CIN.pdf", Type = "application/pdf", Contenu = new byte[] { 0, 1, 2 }, DemandeId = d1.Id });
                _context.Documents.Add(new Document { Nom = "Acte_Naissance.pdf", Type = "application/pdf", Contenu = new byte[] { 3, 4, 5 }, DemandeId = d1.Id });
            }

            // Request 2: Ahmed Permis (EnAttente)
            if (ahmed != null && servicePermis != null)
            {
                var d2 = new Demande
                {
                    Description = "Demande de renouvellement de mon permis de conduire de catégorie B.",
                    Statut = Statut.EnAttente,
                    CitoyenId = ahmed.Id,
                    ServiceId = servicePermis.Id,
                    AgentId = agentPermis?.Id
                };
                _context.Demandes.Add(d2);
                await _context.SaveChangesAsync();

                _context.Documents.Add(new Document { Nom = "Certificat_Medical.pdf", Type = "application/pdf", Contenu = new byte[] { 6, 7 }, DemandeId = d2.Id });
                _context.Documents.Add(new Document { Nom = "Ancien_Permis.pdf", Type = "application/pdf", Contenu = new byte[] { 8, 9 }, DemandeId = d2.Id });
            }

            // Request 3: Sofia Birth Certificate (Traitee)
            if (sofia != null && serviceEC != null)
            {
                var d3 = new Demande
                {
                    Description = "Demande de délivrance d'un extrait d'acte de naissance multilingue.",
                    Statut = Statut.Traitee,
                    CitoyenId = sofia.Id,
                    ServiceId = serviceEC.Id,
                    AgentId = agentEC?.Id
                };
                _context.Demandes.Add(d3);
                await _context.SaveChangesAsync();

                _context.Documents.Add(new Document { Nom = "Copie_CIN.pdf", Type = "application/pdf", Contenu = new byte[] { 10 }, DemandeId = d3.Id });
            }

            // Request 4: Sofia CIVP Contract (EnAttente)
            if (sofia != null && serviceCivp != null)
            {
                var d4 = new Demande
                {
                    Description = "Dépôt d'un nouveau contrat CIVP avec l'entreprise IdaraTech.",
                    Statut = Statut.EnAttente,
                    CitoyenId = sofia.Id,
                    ServiceId = serviceCivp.Id,
                    AgentId = agentAneti?.Id
                };
                _context.Demandes.Add(d4);
                await _context.SaveChangesAsync();

                _context.Documents.Add(new Document { Nom = "Diplome.pdf", Type = "application/pdf", Contenu = new byte[] { 11 }, DemandeId = d4.Id });
                _context.Documents.Add(new Document { Nom = "Contrat_CIVP_Signe.pdf", Type = "application/pdf", Contenu = new byte[] { 12 }, DemandeId = d4.Id });
            }

            // Request 5: Kamel CNSS (Refusee)
            if (kamel != null && serviceCnss != null)
            {
                var d5 = new Demande
                {
                    Description = "Demande d'affiliation en tant que travailleur indépendant.",
                    Statut = Statut.Refusee,
                    CitoyenId = kamel.Id,
                    ServiceId = serviceCnss.Id,
                    AgentId = agentSocial?.Id
                };
                _context.Demandes.Add(d5);
                await _context.SaveChangesAsync();

                _context.Documents.Add(new Document { Nom = "Patente_Fiscale.pdf", Type = "application/pdf", Contenu = new byte[] { 13 }, DemandeId = d5.Id });
            }

            await _context.SaveChangesAsync();
        }

        // 4. Seed Requests for Leila (citoyen4)
        var leila = await _context.Citoyens.FirstOrDefaultAsync(c => c.Email == "citoyen4@localhost");
        if (leila != null && !_context.Demandes.Any(d => d.CitoyenId == leila.Id))
        {
            var agentPasseport = await _context.Agents.FirstOrDefaultAsync(a => a.Email == "agent.passeport@localhost");
            var agentCommune = await _context.Agents.FirstOrDefaultAsync(a => a.Email == "agent.commune@localhost");
            var servicePasseport = await _context.Services.FirstOrDefaultAsync(s => s.Nom.Contains("Passeport"));
            var serviceCommune = await _context.Services.FirstOrDefaultAsync(s => s.Nom.Contains("Mariage"));

            if (servicePasseport != null)
            {
                var d6 = new Demande
                {
                    Description = "Demande de renouvellement de passeport ordinaire.",
                    Statut = Statut.EnCours,
                    CitoyenId = leila.Id,
                    ServiceId = servicePasseport.Id,
                    AgentId = agentPasseport?.Id
                };
                _context.Demandes.Add(d6);
                await _context.SaveChangesAsync();

                _context.Documents.Add(new Document { Nom = "Copie_CIN.pdf", Type = "application/pdf", Contenu = new byte[] { 20, 21 }, DemandeId = d6.Id });
                _context.Documents.Add(new Document { Nom = "Photo_Identite.jpg", Type = "image/jpeg", Contenu = new byte[] { 22, 23 }, DemandeId = d6.Id });
            }

            if (serviceCommune != null)
            {
                var d7 = new Demande
                {
                    Description = "Dépôt de dossier pour la célébration de mariage civil.",
                    Statut = Statut.EnAttente,
                    CitoyenId = leila.Id,
                    ServiceId = serviceCommune.Id,
                    AgentId = agentCommune?.Id
                };
                _context.Demandes.Add(d7);
                await _context.SaveChangesAsync();

                _context.Documents.Add(new Document { Nom = "Extrait_Naissance_Leila.pdf", Type = "application/pdf", Contenu = new byte[] { 24 }, DemandeId = d7.Id });
                _context.Documents.Add(new Document { Nom = "Extrait_Naissance_Conjoint.pdf", Type = "application/pdf", Contenu = new byte[] { 25 }, DemandeId = d7.Id });
                _context.Documents.Add(new Document { Nom = "Certificat_Medical.pdf", Type = "application/pdf", Contenu = new byte[] { 26 }, DemandeId = d7.Id });
            }
            await _context.SaveChangesAsync();
        }

        // 5. Seed Requests for Oussema (citoyen5)
        var oussema = await _context.Citoyens.FirstOrDefaultAsync(c => c.Email == "citoyen5@localhost");
        if (oussema != null && !_context.Demandes.Any(d => d.CitoyenId == oussema.Id))
        {
            var agentCnam = await _context.Agents.FirstOrDefaultAsync(a => a.Email == "agent.cnam@localhost");
            var agentKarama = await _context.Agents.FirstOrDefaultAsync(a => a.Email == "agent.karama@localhost");
            var serviceCnam = await _context.Services.FirstOrDefaultAsync(s => s.Nom.Contains("CNAM"));
            var serviceKarama = await _context.Services.FirstOrDefaultAsync(s => s.Nom.Contains("Karama"));

            if (serviceCnam != null)
            {
                var d8 = new Demande
                {
                    Description = "Nouvelle affiliation à la CNAM suite à un recrutement.",
                    Statut = Statut.EnCours,
                    CitoyenId = oussema.Id,
                    ServiceId = serviceCnam.Id,
                    AgentId = agentCnam?.Id
                };
                _context.Demandes.Add(d8);
                await _context.SaveChangesAsync();

                _context.Documents.Add(new Document { Nom = "Contrat_Travail.pdf", Type = "application/pdf", Contenu = new byte[] { 30, 31 }, DemandeId = d8.Id });
                _context.Documents.Add(new Document { Nom = "Copie_CIN.pdf", Type = "application/pdf", Contenu = new byte[] { 32 }, DemandeId = d8.Id });
            }

            if (serviceKarama != null)
            {
                var d9 = new Demande
                {
                    Description = "Dépôt d'un dossier de contrat Al Karama pour mon recrutement.",
                    Statut = Statut.Traitee,
                    CitoyenId = oussema.Id,
                    ServiceId = serviceKarama.Id,
                    AgentId = agentKarama?.Id
                };
                _context.Demandes.Add(d9);
                await _context.SaveChangesAsync();

                _context.Documents.Add(new Document { Nom = "Diplome_Master.pdf", Type = "application/pdf", Contenu = new byte[] { 33 }, DemandeId = d9.Id });
                _context.Documents.Add(new Document { Nom = "Attestation_Inscription_ANETI.pdf", Type = "application/pdf", Contenu = new byte[] { 34 }, DemandeId = d9.Id });
            }
            await _context.SaveChangesAsync();
        }

        // 6. Seed Requests for Rym (citoyen6)
        var rym = await _context.Citoyens.FirstOrDefaultAsync(c => c.Email == "citoyen6@localhost");
        if (rym != null && !_context.Demandes.Any(d => d.CitoyenId == rym.Id))
        {
            var agentSocial = await _context.Agents.FirstOrDefaultAsync(a => a.Email == "agent.social@localhost");
            var serviceCnssPension = await _context.Services.FirstOrDefaultAsync(s => s.Nom.Contains("Pension de Retraite"));

            if (serviceCnssPension != null)
            {
                var d10 = new Demande
                {
                    Description = "Demande de liquidation de ma pension de retraite.",
                    Statut = Statut.EnAttente,
                    CitoyenId = rym.Id,
                    ServiceId = serviceCnssPension.Id,
                    AgentId = agentSocial?.Id
                };
                _context.Demandes.Add(d10);
                await _context.SaveChangesAsync();

                _context.Documents.Add(new Document { Nom = "Releve_Carrieres.pdf", Type = "application/pdf", Contenu = new byte[] { 40 }, DemandeId = d10.Id });
            }
            await _context.SaveChangesAsync();
        }
    }
}
