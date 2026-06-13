namespace Assura.Application.Features.Transfers.DTOs;
    public class TransferCountsDto
    {
        public int OutgoingCount { get; set; }
        public int IncomingCount { get; set; }
        public int PendingCount { get; set; }
        public int ActiveCount { get; set; }
        public int CompletedCount { get; set; }
    }
