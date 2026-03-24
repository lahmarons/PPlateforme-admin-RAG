namespace IdaraTech.Domain.Events;

public class UtilisateurCreatedEvent : BaseEvent
{
    public UtilisateurCreatedEvent(Utilisateur utilisateur)
    {
        Utilisateur = utilisateur;
    }

    public Utilisateur Utilisateur { get; }
}
