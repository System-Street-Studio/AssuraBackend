using AutoMapper;
using Assura.Domain.Entities;
using Assura.Application.Features.Assets.DTOs;

namespace Assura.Application.Features.Assets;

public class AssetMappingProfile : Profile
{
    public AssetMappingProfile()
    {
        CreateMap<Asset, AssetDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Product != null ? s.Product.Name : s.AssetCode))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
            .ForMember(d => d.SerialNumber, opt => opt.MapFrom(s => s.SerialNumber ?? string.Empty))
            .ForMember(d => d.Division, opt => opt.MapFrom(s => s.Division != null ? s.Division.Name : string.Empty))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
    }
}
