namespace Domain.DTO;

public class UploadFileDto
{
    public int year { get; set; }
    public int month { get; set; }
    public string? description { get; set; }

    public UploadFileDto(int year,
        int month,
        string? description)
    {
        this.year = year % 1000; 
        this.month = month;
        this.description = String.IsNullOrEmpty(description) ? null : description;
    }
}

