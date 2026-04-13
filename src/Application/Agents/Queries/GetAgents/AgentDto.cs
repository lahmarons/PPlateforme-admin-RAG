namespace IdaraTech.Application.Agents.Queries.GetAgents;

public class AgentDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Matricule { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ServiceId { get; set; }
    public string ServiceNom { get; set; } = string.Empty;
}
