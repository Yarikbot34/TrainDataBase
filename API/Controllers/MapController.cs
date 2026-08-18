using Domain.Classes;
using Domain.DTO;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace API.Controllers;

[ApiController]
[Route("api/v1/map")]
public class MapController : ControllerBase
{
    private readonly IMapService _mapService;
    
    public MapController(IMapService mapService)
    {
        this._mapService = mapService;
    }

    [HttpPost("uploadSchema")]
    public async Task<IActionResult> UploadSchema(MapSchema schema)
    {
        await _mapService.UploadMapSchemaAsync(schema);
        return Ok();
    }

    [HttpPost("getSchema")]
    public async Task<IActionResult> GetSchemaAsync(MapRequestDto req)
    {
        var asnw = await _mapService.GetMapSchemaAsync(req.schemaName,req.years, req.months);
        return Ok(asnw);
    }
}