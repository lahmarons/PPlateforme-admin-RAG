using IdaraTech.Application.Demandes.Queries.GetDemandById;
using IdaraTech.Domain.Entities;

namespace IdaraTech.Application.Common.Mappings;

public class DemandeMappingProfile : Profile
{
    public DemandeMappingProfile()
    {
        CreateMap<Demande, DemandeDto>()
            .ForMember(d => d.CitoyenNom, opt => opt.MapFrom(s => s.Citoyen != null ? s.Citoyen.Nom : string.Empty))
            .ForMember(d => d.ServiceNom, opt => opt.MapFrom(s => s.ServiceConcerne != null ? s.ServiceConcerne.Nom : string.Empty))
            .ForMember(d => d.AgentNom, opt => opt.MapFrom(s => s.AgentTraitant != null ? s.AgentTraitant.Nom : string.Empty))
            .ForMember(d => d.Statut, opt => opt.MapFrom(s => (int)s.Statut))
            .ForMember(d => d.StatutName, opt => opt.MapFrom(s => s.Statut.ToString()));

        CreateMap<Document, DocumentDto>();
    }
}
