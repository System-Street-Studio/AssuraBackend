using AutoMapper;
using Assura.Domain.Entities;
using Assura.Application.Features.LostItems.DTOs;

namespace Assura.Application.Features.LostItems;

public class LostItemMappingProfile : Profile
{
    public LostItemMappingProfile()
    {
        CreateMap<LostItem, LostItemDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.AssetId, opt => opt.MapFrom(s => s.AssetId))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => MapStatus(s.Status)))
            .ForMember(d => d.Date, opt => opt.MapFrom(s => s.Date.ToString("dd MMM yyyy")))
            .ForMember(d => d.Time, opt => opt.MapFrom(s => s.Time.ToString(@"hh\:mm")))
            .ForMember(d => d.ValueAtPurchasing, opt => opt.MapFrom(s => s.ValueAtPurchasing.ToString("N0")))
            .ForMember(d => d.CurrentValue, opt => opt.MapFrom(s => s.CurrentValue.ToString("N0")));
    }

    private static string MapStatus(Domain.Enums.LostItemStatus status) => status switch
    {
        Domain.Enums.LostItemStatus.Reported => "Reported",
        Domain.Enums.LostItemStatus.UnderInvestigation => "Under Investigation",
        Domain.Enums.LostItemStatus.ConfirmedLost => "Confirmed Lost",
        _ => status.ToString()
    };
}
