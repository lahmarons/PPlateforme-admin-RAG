namespace IdaraTech.Domain.Entities;

public class Prestataire : BaseAuditableEntity
{
    public string Nom { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IList<Service> Services { get; private set; } = new List<Service>();
}
