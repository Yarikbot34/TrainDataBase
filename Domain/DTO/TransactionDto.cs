using System.Text.Json.Serialization;
using Domain.Classes;

namespace Domain.DTO;

public class TransactionDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime DateCreated { get; set; }
    
    public int UnitCount { get; set; }
    public string UserName { get; set; }
    public string Description { get; set; }
    public string TransactionType { get; set; }
    
    [JsonConstructor]
    public TransactionDto(){}
    
    public TransactionDto(Transaction transaction)
    {
        Id = transaction.Id;
        Year = transaction.Year;
        Month = transaction.Month;
        DateCreated = transaction.GetDateTime();
        UnitCount = transaction.UnitsGet;
        UserName = transaction.User is null ? "Удален" : transaction.User.Username;
        Description = transaction.Description;
        TransactionType = transaction.Type.ToString();
    }
}