namespace IdaraTech.Domain.Entities;

public class Service : BaseAuditableEntity
{
    public string Nom { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int? PrestataireId { get; set; }

    public Prestataire? Prestataire { get; set; }

    public IList<Agent> Agents { get; private set; } = new List<Agent>();

    public IList<Demande> Demandes { get; private set; } = new List<Demande>();
}
