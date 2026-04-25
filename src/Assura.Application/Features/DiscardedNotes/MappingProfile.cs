using AutoMapper;
using Assura.Domain.Entities;
using Assura.Application.Features.DiscardedNotes.DTOs;

namespace Assura.Application.Features.DiscardedNotes;

public class DiscardedNotesMappingProfile : Profile
{
    public DiscardedNotesMappingProfile()
    {
        CreateMap<DiscardedNote, DiscardedNoteDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => MapStatus(s.Status)))
            .ForMember(d => d.Date, opt => opt.MapFrom(s => s.Date.ToString("dd MMM yyyy")))
            .ForMember(d => d.Time, opt => opt.MapFrom(s => s.Time.ToString(@"hh\:mm")));
    }

    private static string MapStatus(Domain.Enums.DiscardNoteStatus status) => status switch
    {
        Domain.Enums.DiscardNoteStatus.Pending => "Pending",
        Domain.Enums.DiscardNoteStatus.InProgress => "In Progress",
        Domain.Enums.DiscardNoteStatus.Completed => "Completed",
        Domain.Enums.DiscardNoteStatus.Rejected => "Rejected",
        _ => status.ToString()
    };
}
