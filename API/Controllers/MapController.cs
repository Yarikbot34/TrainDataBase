using Domain.Classes;
using Domain.DTO;
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

    [HttpPost("getSchema/{schemaName}")]
    public async Task<IActionResult> GetSchemaAsync(MapRequestDto req)
    {
        var asnw = await _mapRepo.GetMapSchemaAsync(req.schemaName,req.years, req.months);
        return Ok(asnw);
    }
}