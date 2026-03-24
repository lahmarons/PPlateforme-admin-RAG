namespace IdaraTech.Domain.Events;

public class DemandeStatusChangedEvent : BaseEvent
{
    public DemandeStatusChangedEvent(Demande demande, Statut previousStatus)
    {
        Demande = demande;
        PreviousStatus = previousStatus;
    }

    public Demande Demande { get; }

    public Statut PreviousStatus { get; }
}
