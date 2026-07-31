namespace Domain.Classes;

public class Transaction
{
    public int  Id {get; set;}
    
    public int Year {get; set;}
    public int Month {get; set;}
    
    public DateOnly Date {get; set;} =  DateOnly.FromDateTime(DateTime.Now);
    public TimeOnly Time {get; set;} =  TimeOnly.FromDateTime(DateTime.Now);
    
    public string Description {get; set;} = "";
    
    public int UnitsGet {get; set;}
}