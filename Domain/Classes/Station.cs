namespace Domain.Classes;

public class Station
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Class { get; set; }
    
    public int? TransactionId { get; set; }
    public Transaction? Transaction { get; set; }
}