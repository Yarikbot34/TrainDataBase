using Microsoft.AspNetCore.Http;
namespace Domain.DTO;

public class UploadFileDto
{
    public int year { get; set; }
    public int month { get; set; }
    public string? description { get; set; }
    public IFormFile  file { get; set; }
}

