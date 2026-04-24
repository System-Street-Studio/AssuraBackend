using Microsoft.AspNetCore.Mvc;
using Assura.API.Controllers; 
using Microsoft.AspNetCore.Authorization;
using Assura.Domain.Enums;

namespace Assura.WebAPI.Controllers;

[AllowAnonymous]
[Route("api/[controller]")] 
[ApiController]
public class TransfersController : BaseApiController 
{
   /* [HttpGet]
    public IActionResult GetTransfers()
    {
        return Ok("Transfers endpoint is available. Transfer-related features have been temporarily removed.");
    }*/
}