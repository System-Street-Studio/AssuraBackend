using AutoMapper;
using Assura.Domain.Entities;
using Assura.Application.Features.Receipts.DTOs;

namespace Assura.Application.Features.Receipts;

public class ReceiptMappingProfile : Profile
{
    public ReceiptMappingProfile()
    {
        CreateMap<Receipt, ReceiptDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Date, opt => opt.MapFrom(s => s.Date.ToString("dd MMM yyyy")));
    }
}
