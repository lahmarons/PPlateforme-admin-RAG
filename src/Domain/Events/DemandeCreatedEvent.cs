namespace IdaraTech.Domain.Events;

public class DemandeCreatedEvent : BaseEvent
{
    public DemandeCreatedEvent(Demande demande)
    {
        Demande = demande;
    }

    public Demande Demande { get; }
}
