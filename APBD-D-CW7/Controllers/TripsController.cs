using APBD_D_CW7.Exceptions;
using APBD_D_CW7.Models.DTOs;
using APBD_D_CW7.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace APBD_D_CW7.Controllers;
[ApiController]
[Route("api/[controller]")]
public class TripsController(IDbService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllTrips()
    {
        return Ok(await service.GetTripsAsync());
    }
    
    
}