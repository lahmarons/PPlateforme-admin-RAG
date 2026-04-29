namespace IdaraTech.Application.Services.Queries.GetServices;

public class ServiceDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int? PrestataireId { get; set; }
    public string PrestataireNom { get; set; } = string.Empty;
}
