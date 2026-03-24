namespace IdaraTech.Application.Demandes.Queries.GetDemandById;

public class DemandeDto
{
    public int Id { get; set; }

    public int CitoyenId { get; set; }

    public string CitoyenNom { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Statut { get; set; }

    public string StatutName { get; set; } = string.Empty;

    public int? ServiceId { get; set; }

    public string? ServiceNom { get; set; }

    public int? AgentId { get; set; }

    public string? AgentNom { get; set; }

    public IList<DocumentDto> Documents { get; set; } = new List<DocumentDto>();
}

public class DocumentDto
{
    public int Id { get; set; }

    public string Nom { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}
