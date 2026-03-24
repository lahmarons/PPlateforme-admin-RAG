namespace IdaraTech.Domain.Entities;

public class Admin : Utilisateur
{
    public IList<Service> ServicesGeres { get; private set; } = new List<Service>();
}
