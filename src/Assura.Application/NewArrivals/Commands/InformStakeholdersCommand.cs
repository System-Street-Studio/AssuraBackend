using Assura.Application.Common.Interfaces;
using Assura.Application.DTOs;
using Assura.Domain.Entities;
using Assura.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.NewArrivals.Commands;

public record InformStakeholdersCommand(InformStakeholdersDto Dto) : IRequest<int>;

public class InformStakeholdersCommandHandler : IRequestHandler<InformStakeholdersCommand, int>
{
    private readonly IApplicationDbContext _context;

    public InformStakeholdersCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(InformStakeholdersCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var informing = await _context.AssetInformings
            .Include(a => a.Division)
            .FirstOrDefaultAsync(a => a.Id == dto.InformingId, cancellationToken);

        if (informing == null)
            throw new Exception("Asset Informing record not found");

        informing.Status = "Informed";

        var employee = await _context.Users.FindAsync(new object[] { dto.EmployeeId }, cancellationToken);
        if (employee != null)
        {
            _context.Notifications.Add(new Notification
            {
                Title = "New Asset Arrival",
                Message = $"A new '{informing.ItemName}' has arrived for you. {(string.IsNullOrEmpty(dto.Remarks) ? "" : $"Remarks: {dto.Remarks}")}",
                UserId = employee.Id,
                Type = "Info",
                ReferenceId = informing.Id.ToString()
            });
        }

        if (dto.DivisionHeadNotify)
        {
            var divisionHeads = await _context.Users
                .Where(u => u.Role == UserRole.DivisionHead && u.DivisionId == informing.DivisionId)
                .ToListAsync(cancellationToken);

            foreach (var head in divisionHeads)
            {
                _context.Notifications.Add(new Notification
                {
                    Title = "Asset Arrival for Division",
                    Message = $"A new '{informing.ItemName}' has arrived for your division (Assigned to {employee?.FirstName} {employee?.LastName}).",
                    UserId = head.Id,
                    Type = "Info",
                    ReferenceId = informing.Id.ToString()
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return informing.Id;
    }
}
