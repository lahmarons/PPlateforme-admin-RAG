namespace IdaraTech.Domain.Entities;

public class Service : BaseAuditableEntity
{
    public string Nom { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public IList<Agent> Agents { get; private set; } = new List<Agent>();

    public IList<Demande> Demandes { get; private set; } = new List<Demande>();
}
