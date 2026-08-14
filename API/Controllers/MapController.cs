using Domain.Classes;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace API.Controllers;

[ApiController]
[Route("api/v1/map")]
public class MapController : ControllerBase
{
    private readonly IMapRepo _mapRepo;
    
    public MapController(IMapRepo mapRepo)
    {
        this._mapRepo = mapRepo;
    }

    [HttpPost("uploadSchema")]
    public async Task<IActionResult> UploadSchema(MapSchema schema)
    {
        await _mapRepo.UploadMapSchemaAsync(schema);
        return Ok();
    }
}