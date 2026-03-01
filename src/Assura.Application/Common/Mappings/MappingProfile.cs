using AutoMapper;
using Assura.Application.DTOs;
using Assura.Domain.Entities;

namespace Assura.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Division, DivisionDto>().ReverseMap();
    }
}
