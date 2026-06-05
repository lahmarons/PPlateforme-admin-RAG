namespace IdaraTech.Application.Documents.Queries.GetDocuments;

public class DocumentDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public byte[] Contenu { get; set; } = Array.Empty<byte>();
    public int DemandeId { get; set; }
}
