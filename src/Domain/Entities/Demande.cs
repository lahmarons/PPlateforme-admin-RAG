using IdaraTech.Domain.Enums;

namespace IdaraTech.Domain.Entities;

public class Demande : BaseAuditableEntity
{
    public string Description { get; set; } = string.Empty;

    public Statut Statut { get; set; } = Statut.EnAttente;

    public int CitoyenId { get; set; }

    public Citoyen? Citoyen { get; set; }

    public int? ServiceId { get; set; }

    public Service? ServiceConcerne { get; set; }

    public int? AgentId { get; set; }

    public Agent? AgentTraitant { get; set; }

    public IList<Document> Documents { get; private set; } = new List<Document>();

    public void ChangerStatut(Statut nouveauStatut)
    {
        Statut = nouveauStatut;
    }
}
