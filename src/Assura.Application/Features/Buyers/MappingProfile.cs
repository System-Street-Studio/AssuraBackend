using AutoMapper;
using Assura.Domain.Entities;
using Assura.Application.Features.Buyers.DTOs;

namespace Assura.Application.Features.Buyers;

public class BuyerMappingProfile : Profile
{
    public BuyerMappingProfile()
    {
        CreateMap<Buyer, BuyerDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
    }
}
