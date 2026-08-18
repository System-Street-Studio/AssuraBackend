using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Assura.Application.Common.Interfaces;
using Assura.Application.Features.Receipts.DTOs;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Receipts.Commands.UploadFile;

public record UploadReceiptFileCommand(string ReceiptId, string FileUrl) : IRequest<ReceiptDto?>;

public class UploadReceiptFileCommandHandler : IRequestHandler<UploadReceiptFileCommand, ReceiptDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UploadReceiptFileCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ReceiptDto?> Handle(UploadReceiptFileCommand request, CancellationToken cancellationToken)
    {
        var receipt = await _context.Receipts.FirstOrDefaultAsync(r => r.Id.ToString() == request.ReceiptId, cancellationToken);
        if (receipt == null)
        {
            return null;
        }

        receipt.FileUrl = request.FileUrl;
        receipt.Status = ReceiptStatus.Uploaded;
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ReceiptDto>(receipt);
    }
}
