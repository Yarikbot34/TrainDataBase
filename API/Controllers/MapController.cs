using Domain.Classes;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace API.Controllers;

[ApiController]
[Route("api/v1/map")]
public class MapController : ControllerBase
{
    private readonly IMapService mapService;
    
    public MapController(IMapService mapService)
    {
        this.mapService = mapService;
    }

    [HttpPost("uploadSchema")]
    public async Task<IActionResult> UploadSchema(MapSchema schema)
    {
        await mapService.UploadMapSchemaAsync(schema);
        return Ok();
    }
}