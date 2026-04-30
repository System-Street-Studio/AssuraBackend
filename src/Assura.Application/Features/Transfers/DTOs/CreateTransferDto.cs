using System.ComponentModel.DataAnnotations;

namespace Assura.Application.Features.Transfers.DTOs;

public class CreateTransferDto
{

    [Required]
    public int AssetId { get; set; }

    [Required]
    public int AssetRequestId { get; set; }
    
   

}

