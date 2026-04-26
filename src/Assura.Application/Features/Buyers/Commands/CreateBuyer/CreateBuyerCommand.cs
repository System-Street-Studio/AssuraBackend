using MediatR;
using Assura.Domain.Enums;

namespace Assura.Application.Features.Buyers.Commands.CreateBuyer;

public class CreateBuyerCommand : IRequest<string>
{
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
