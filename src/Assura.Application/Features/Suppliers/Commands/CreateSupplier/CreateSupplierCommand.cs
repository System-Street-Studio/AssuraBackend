using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using MediatR;

namespace Assura.Application.Features.Suppliers.Commands.CreateSupplier;

public record CreateSupplierCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    public string? ContactPerson { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? Website { get; init; }
}

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateSupplierCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var entity = new Supplier
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Website = request.Website
        };

        _context.Suppliers.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
        catch (Exception)
        {
            // Log the exception if you have a logger here, otherwise rethrow
            throw;
        }
    }
}
