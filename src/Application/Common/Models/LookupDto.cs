using IdaraTech.Domain.Entities;

namespace IdaraTech.Application.Common.Models;

public class LookupDto
{
    public int Id { get; init; }

    public string? Title { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Service, LookupDto>()
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Nom));

            CreateMap<Utilisateur, LookupDto>()
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Nom));

            CreateMap<Demande, LookupDto>()
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Description));

            CreateMap<Document, LookupDto>()
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Nom));
        }
    }
}
