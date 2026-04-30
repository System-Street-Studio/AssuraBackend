using MediatR;
using Assura.Application.Features.Transfers.DTOs;

namespace Assura.Application.Features.Transfers.Queries;

public record GetDivisionHeadTransferQuery(string Tab, int LoginUserId) : IRequest<List<TransferDto>>;