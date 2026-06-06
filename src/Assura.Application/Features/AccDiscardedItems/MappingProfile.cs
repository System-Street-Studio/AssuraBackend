using AutoMapper;
using Assura.Domain.Entities;
using Assura.Application.Features.AccDiscardedItems.DTOs;

namespace Assura.Application.Features.AccDiscardedItems;

public class AccDiscardedItemMappingProfile : Profile
{
    public AccDiscardedItemMappingProfile()
    {
        CreateMap<AccDiscardedItem, AccDiscardedItemDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Date, opt => opt.MapFrom(s => s.Date.ToString("dd MMM yyyy")))
            .ForMember(d => d.Time, opt => opt.MapFrom(s => s.Time.ToString(@"hh\:mm")))
            .ForMember(d => d.ValueAtPurchasing, opt => opt.MapFrom(s => s.ValueAtPurchasing.ToString("N0")))
            .ForMember(d => d.CurrentValue, opt => opt.MapFrom(s => s.CurrentValue.ToString("N0")));
    }
}
