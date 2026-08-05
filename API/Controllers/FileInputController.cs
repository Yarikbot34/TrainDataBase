using Microsoft.AspNetCore.Mvc;
using TableReader;

namespace API.Controllers;


[ApiController]
[Route("api/v1/file")]
public class InputFileController : ControllerBase
{
    private readonly ITableReader _tableReader;
    
    public InputFileController(ITableReader tableReader)
    {
        _tableReader = tableReader;
    }

    [HttpPost("input")]
    public async Task<IActionResult> InputDataFromFile(IFormFile file, int year, int month)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), file.FileName);
        
        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        try
        {
            var trainWithNoDesc = await _tableReader.ExtractFromFile(fs, year, month);
            return Ok(trainWithNoDesc);
            
        } catch (Exception ex)
        {
            return Content(ex.Message);
        }
        finally
        {
            fs.Close();
            System.IO.File.Delete(path);
        }
        
        
    }
    
}
