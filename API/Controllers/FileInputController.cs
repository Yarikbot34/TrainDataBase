using Domain.DTO;
using Microsoft.AspNetCore.Mvc;
using Services;
using FileWorker;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;


[ApiController]
[Route("api/v1/file")]
[Authorize(Roles = "Admin, Upload")]
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
    public async Task<IActionResult> InputDataFromFile(UploadFileDto uploadDto)
    {
        var file = uploadDto.file;
        uploadDto.year = uploadDto.year > 1000 ? uploadDto.year % 1000 : uploadDto.year;
        uploadDto.description = String.IsNullOrEmpty(uploadDto.description) ? "" : uploadDto.description;
        int yearlng = uploadDto.year + 2000;
        if (yearlng > DateTime.Today.Year || (yearlng == DateTime.Today.Year && uploadDto.month > DateTime.Today.Month))
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
            var trainWithNoDesc = await _fileWorker.ExtractFromFile(fs, uploadDto, User);
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
        if (User.Identity is not null)
        {
            await _trainService.AddTrainDescById(id, dto, User);
            return Ok();
        }
        else return BadRequest("Ошибка авторизации");
    }
    
}
