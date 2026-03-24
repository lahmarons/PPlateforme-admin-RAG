namespace IdaraTech.Domain.Entities;

public class Citoyen : Utilisateur
{
    public string Adresse { get; set; } = string.Empty;

    public string Telephone { get; set; } = string.Empty;

    public IList<Demande> Demandes { get; private set; } = new List<Demande>();
}
