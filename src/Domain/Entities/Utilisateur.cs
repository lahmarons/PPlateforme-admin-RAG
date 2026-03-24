namespace IdaraTech.Domain.Entities;

public abstract class Utilisateur : BaseAuditableEntity
{
    public string Nom { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string MotDePasse { get; set; } = string.Empty;
}
