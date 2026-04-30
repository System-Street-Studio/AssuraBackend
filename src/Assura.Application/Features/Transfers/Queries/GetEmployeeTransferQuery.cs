using MediatR;
using Assura.Application.Features.Transfers.DTOs;

namespace Assura.Application.Features.Transfers.Queries;

public record GetEmployeeTransferQuery(string Tab, int LoginUserId) : IRequest<List<TransferDto>>;