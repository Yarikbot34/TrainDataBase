using Domain.DTO;
using Microsoft.AspNetCore.Mvc;
using Services;
using TableReader;

namespace API.Controllers;


[ApiController]
[Route("api/v1/file")]
public class InputFileController : ControllerBase
{
    private readonly ITableReader _tableReader;
    private readonly ITrainRepo _trainRepo;
    
    public InputFileController(ITableReader tableReader, ITrainRepo trainRepo)
    {
        _tableReader = tableReader;
        _trainRepo = trainRepo;
    }

    [HttpPost("input")]
    public async Task<IActionResult> InputDataFromFile(IFormFile file, int year, int month)
    {
        if (year > DateTime.Today.Year || month > DateTime.Today.Month)
        {
            return BadRequest("Этот период ещё не прожит");
        }
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

    [HttpPatch("input/addDesc/{id}")]
    public async Task<IActionResult> UpdateTrainDesc(int id, TrainDto dto)
    {
        _trainRepo.AddTrainDescById(id, dto);
        return Ok();
    }
    
}
