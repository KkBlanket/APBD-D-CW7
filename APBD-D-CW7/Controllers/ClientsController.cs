using APBD_D_CW7.Exceptions;
using APBD_D_CW7.Models.DTOs;
using APBD_D_CW7.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_D_CW7.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ClientsController(IDbService service) : ControllerBase
{
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetTripsByClientIdAsync(int id)
    {
        try
        {
            return Ok(await service.GetTripsByClientId(id));
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
    }
}