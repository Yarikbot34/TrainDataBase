using Domain.DTO;
using Microsoft.AspNetCore.Mvc;
using Services;
using FileWorker;

namespace API.Controllers;


[ApiController]
[Route("api/v1/file")]
public class InputFileController : ControllerBase
{
    private readonly IFileWorker _fileWorker;
    private readonly ITrainService _trainService;
    
    public InputFileController(IFileWorker fileWorker, ITrainService trainService)
    {
        _fileWorker = fileWorker;
        _trainService = trainService;
    }

    [HttpPost("input")]
    public async Task<IActionResult> InputDataFromFile(IFormFile file, int year, int month)
    {
        int yearlng = year + 2000;
        if (yearlng > DateTime.Today.Year || (yearlng == DateTime.Today.Year && month > DateTime.Today.Month))
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
            var trainWithNoDesc = await _fileWorker.ExtractFromFile(fs, year, month);
            return Ok(trainWithNoDesc);
            
        } catch (Exception ex)
        {
            return Conflict(ex.Message);
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
        _trainService.AddTrainDescById(id, dto);
        return Ok();
    }
    
}
