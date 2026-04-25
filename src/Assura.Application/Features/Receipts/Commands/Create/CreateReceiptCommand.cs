using MediatR;
using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Receipts.DTOs;
using Assura.Domain.Entities;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Receipts.Commands.Create;

public class CreateReceiptCommand : IRequest<ReceiptDto>
{
    public string AssetName { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
}

public class CreateReceiptCommandHandler : IRequestHandler<CreateReceiptCommand, ReceiptDto>
{
    private readonly IApplicationDbContext _context;

    public CreateReceiptCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReceiptDto> Handle(CreateReceiptCommand request, CancellationToken cancellationToken)
    {
        var entity = new Receipt
        {
            AssetName = request.AssetName,
            Division = request.Division,
            Date = DateTime.TryParse(request.Date, out var dt) ? dt : DateTime.UtcNow,
            Amount = request.Amount,
            Status = ReceiptStatus.Pending
        };

        _context.Receipts.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new ReceiptDto
        {
            Id = entity.Id.ToString(),
            AssetName = entity.AssetName,
            Division = entity.Division,
            Date = entity.Date.ToString("dd MMM yyyy"),
            Amount = entity.Amount,
            Status = entity.Status.ToString()
        };
    }
}
