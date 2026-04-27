using AutoMapper;
using Assura.Domain.Entities;
using Assura.Application.Features.AccPendingItems.DTOs;

namespace Assura.Application.Features.AccPendingItems;

public class AccPendingItemMappingProfile : Profile
{
    public AccPendingItemMappingProfile()
    {
        CreateMap<AccPendingItem, AccPendingItemDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category.ToString().ToLower().Replace("tobeapproved", "to-be-approved").Replace("rejected", "reject")))
            .ForMember(d => d.Date, opt => opt.MapFrom(s => s.Date.ToString("yyyy-MM-dd")))
            .ForMember(d => d.Time, opt => opt.MapFrom(s => s.Time.ToString(@"hh\:mm")))
            .ForMember(d => d.ValueAtPurchasing, opt => opt.MapFrom(s => s.ValueAtPurchasing.ToString("N0")))
            .ForMember(d => d.CurrentValue, opt => opt.MapFrom(s => s.CurrentValue.ToString("N0")));
    }
}
