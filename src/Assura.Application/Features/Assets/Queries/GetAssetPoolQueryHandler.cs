using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Assets.DTOs;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using System.Text.Json;

namespace Assura.Application.Features.Assets.Queries;

public class GetAssetPoolQueryHandler : IRequestHandler<GetAssetPoolQuery, AssetPoolFilterResult>
{
    private readonly IApplicationDbContext _context;

    public GetAssetPoolQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssetPoolFilterResult> Handle(GetAssetPoolQuery request, CancellationToken cancellationToken)
    {
        try
        {

                // Build the query for assigned assets - filter to ONLY employees
                var query = _context.Assets
                    .AsNoTracking()
                    .Where(a => a.AssignedUserId != null && a.AssignedUser != null &&
                                !a.AssignedUser.IsDeleted && 
                                a.AssignedUser.IsActive &&
                                a.AssignedUser.Role == UserRole.Employee)
                    .AsQueryable();


                // Apply search filter
                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    var searchLower = request.Search.ToLower();
                    query = query.Where(a =>
                        (a.Product != null && a.Product.Name.ToLower().Contains(searchLower)) ||
                        a.AssetCode.ToLower().Contains(searchLower) ||
                        (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(searchLower)) ||
                        (a.Division != null && a.Division.Name.ToLower().Contains(searchLower)) ||
                        (a.AssignedUser != null && (a.AssignedUser.FirstName.ToLower().Contains(searchLower) ||
                                                   a.AssignedUser.LastName.ToLower().Contains(searchLower)))
                    );
                }

                // Apply category filter (exact match)
                if (!string.IsNullOrWhiteSpace(request.Category))
                {
                    var categoryName = request.Category.Trim();
                    query = query.Where(a => a.Category != null && a.Category.Name == categoryName);
                  
                }

                // Apply division filter (exact match) - from asset's DivisionId
                if (!string.IsNullOrWhiteSpace(request.Division))
                {
                    var divisionName = request.Division.Trim();
                    query = query.Where(a => a.Division != null && a.Division.Name == divisionName);
               
                }

                // Apply employee filter
                if (request.EmployeeId.HasValue && request.EmployeeId.Value > 0)
                {
                    query = query.Where(a => a.AssignedUserId == request.EmployeeId.Value);
                   
                }

                // Apply specification filter
                if (!string.IsNullOrWhiteSpace(request.SpecName) && !string.IsNullOrWhiteSpace(request.SpecValue))
                {
                    var specSearchPattern = $"{request.SpecName}:{request.SpecValue}";
                    query = query.Where(a => a.Notes != null && a.Notes.Contains(specSearchPattern));
                   ;
                }

                // Execute query to get total count
               
                var totalCount = await query.CountAsync(cancellationToken);
               

                if (totalCount == 0)
                {
               
                    return new AssetPoolFilterResult
                    {
                        Assets = new List<AssetPoolDto>(),
                        Divisions = await _context.Divisions.AsNoTracking().OrderBy(d => d.Name).Select(d => new DivisionDto { Id = d.Id, Name = d.Name }).ToListAsync(cancellationToken),
                        TotalCount = 0,
                        Page = request.Page,
                        PageSize = request.PageSize
                    };
                }



                var assetDtos = await query
                .OrderBy(a => a.AssetCode)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AssetPoolDto
                {
                    Id = a.Id,
                    ProductName = a.Product != null ? a.Product.Name : (a.AssetCode ?? "Unknown"),
                    AssetCode = a.AssetCode ?? "",
                    AssetTag = a.AssetTag ?? string.Empty,
                    CategoryId = a.CategoryId ?? 0,
                    CategoryName = a.Category != null ? a.Category.Name : string.Empty,
                    AssignedUserId = a.AssignedUserId ?? 0,
                    AssignedUserName = a.AssignedUser != null 
                        ? (a.AssignedUser.FirstName + " " + a.AssignedUser.LastName).Trim() 
                        : string.Empty,
                    DivisionId = a.Division != null ? a.Division.Id : 0,
                    DivisionName = a.Division != null ? a.Division.Name : string.Empty,
                    Status = a.Status.ToString(), 
                    SerialNumber = a.SerialNumber ?? string.Empty,
                    Notes = a.Notes ?? string.Empty,
                    CreatedDate = a.CreatedAt,
                    UpdatedDate = a.UpdatedAt
                })
                .ToListAsync(cancellationToken);

           
            foreach (var dto in assetDtos)
            {
                dto.Specifications = ParseSpecifications(dto.Notes);
            }

            
            return new AssetPoolFilterResult
            {
                Assets = assetDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($" Inner exception: {ex.InnerException.Message}");
               
            }
            throw;
        }
    }

    private Dictionary<string, string> ParseSpecifications(string? notes)
    {
        var specs = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(notes))
            return specs;

        // Parse format: "RAM: 8GB, Storage: 256GB SSD, Processor: Intel i7"
        var pairs = notes.Split(',');
        foreach (var pair in pairs)
        {
            var trimmedPair = pair.Trim();
            var colonIndex = trimmedPair.IndexOf(':');
            if (colonIndex > 0)
            {
                var key = trimmedPair.Substring(0, colonIndex).Trim();
                var value = trimmedPair.Substring(colonIndex + 1).Trim();
                specs[key] = value;
            }
        }

        return specs;
    }
}

