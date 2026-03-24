namespace IdaraTech.Domain.Entities;

public class Document : BaseAuditableEntity
{
    public string Nom { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int DemandeId { get; set; }

    public Demande? Demande { get; set; }
}
