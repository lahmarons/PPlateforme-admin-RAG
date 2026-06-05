namespace IdaraTech.Domain.Entities;

public class Document : BaseAuditableEntity
{
    public string Nom { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public byte[] Contenu { get; set; } = Array.Empty<byte>();

    public int DemandeId { get; set; }

    public Demande? Demande { get; set; }
}
