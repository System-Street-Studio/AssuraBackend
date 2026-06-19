using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;

namespace Assura.Application.Features.AssetSpecifications.Queries;

public record GetAssetSpecificationsQuery(int? CategoryId = null) : IRequest<List<AssetSpecificationDto>>;

public class GetAssetSpecificationsQueryHandler : IRequestHandler<GetAssetSpecificationsQuery, List<AssetSpecificationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAssetSpecificationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssetSpecificationDto>> Handle(GetAssetSpecificationsQuery request, CancellationToken cancellationToken)
    {
        var specifications = new List<AssetSpecificationDto>();
        
        // Get categories first
        var categories = await _context.Categories.ToListAsync(cancellationToken);

        // Define specifications for each category based on asset types
        var categorySpecifications = new Dictionary<string, List<string>>
        {
            { "Computers", new List<string> { "RAM (GB)", "Storage (GB)", "Processor", "Display Size", "GPU", "OS" } },
            { "Servers", new List<string> { "OS", "RAM (GB)", "CPU", "IP Address", "Storage (GB)" } },
            { "Network Equipment", new List<string> { "Port Count", "Data Rate", "Form Factor", "MAC Address" } },
            { "Printers", new List<string> { "Print Technology", "Connectivity", "Print Resolution", "Type" } },
            { "Furniture", new List<string> { "Material", "Length (cm)", "Width (cm)", "Height (cm)" } },
            { "Mobile Devices", new List<string> { "Storage (GB)", "RAM (GB)", "Display Size", "OS", "Battery" } },
            { "Audio/Video", new List<string> { "Resolution", "Connectivity", "Storage (GB)", "Display Size" } },
            { "Office Equipment", new List<string> { "Type", "Condition", "Power (W)", "Dimensions" } }
        };

        int specId = 1;

        foreach (var category in categories)
        {
            // Get specifications for this category if defined
            if (categorySpecifications.TryGetValue(category.Name, out var specs))
            {
                foreach (var spec in specs)
                {
                    specifications.Add(new AssetSpecificationDto
                    {
                        Id = specId++,
                        Name = spec,
                        CategoryId = category.Id,
                        CategoryName = category.Name
                    });
                }
            }
        }

        // Filter by categoryId if provided
        if (request.CategoryId.HasValue)
        {
            specifications = specifications
                .Where(s => s.CategoryId == request.CategoryId.Value)
                .ToList();
        }

        return specifications;
    }
}
