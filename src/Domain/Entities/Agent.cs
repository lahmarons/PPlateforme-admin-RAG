namespace IdaraTech.Domain.Entities;

public class Agent : Utilisateur
{
    public string Matricule { get; set; } = string.Empty;

    public string Grade { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int? ServiceId { get; set; }

    public Service? Service { get; set; }

    public IList<Demande> DemandesToitees { get; private set; } = new List<Demande>();
}
