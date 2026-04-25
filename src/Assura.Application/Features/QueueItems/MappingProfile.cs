using AutoMapper;
using Assura.Domain.Entities;
using Assura.Application.Features.QueueItems.DTOs;

namespace Assura.Application.Features.QueueItems;

public class QueueItemMappingProfile : Profile
{
    public QueueItemMappingProfile()
    {
        CreateMap<QueueItem, QueueItemDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Date, opt => opt.MapFrom(s => s.Date.ToString("yyyy-MM-dd")))
            .ForMember(d => d.Time, opt => opt.MapFrom(s => s.Time.ToString(@"hh\:mm")));
    }
}
