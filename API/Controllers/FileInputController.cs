using Microsoft.AspNetCore.Mvc;
using TableReader;

namespace API.Controllers;

public class FileInputController
{
    [ApiController]
    [Route("api/v1/file/input")]
    public class InputFileController : ControllerBase
    {
        private readonly ITableReader _tableReader;
        
        public InputFileController(ITableReader tableReader)
        {
            _tableReader = tableReader;
        }

        [HttpGet]
        public IActionResult GetFileData()
        {
            var fs = new FileStream("dataTrue.xlsx", FileMode.Open, FileAccess.Read);
            _tableReader.ExtractFromFile(fs, 26, 7);
            return Ok();
        }
        
    }
}