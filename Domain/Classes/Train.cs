namespace Domain.Classes;

public class Train
{
    public int Id { get; set; }
    public int year {get; set;}
    public int month {get; set;}
    public string Number { get; set; }
    
    public bool IsCanceled { get; set; }
    public bool HasDesc { get; set; }
    
    public string? Description { get; set; }
    
    public int RouteId { get; set; }
    public Route Route { get; set; }
    
    //Тех. Данные
    public int StationFromId { get; set; }
    public Station StationFrom { get; set; }
    
    public int? StationMiddleId { get; set; }  
    public Station StationMiddle { get; set; }
    
    public int StationToId { get; set; }
    public Station StationTo { get; set; }
    
    public TimeOnly TimeFrom { get; set; }
    public TimeOnly TimeTo { get; set; }
    
    public int Distance { get; set; }
    public int RailcarCount { get; set; }
    private int _dayInRaise;
    public int DayInRaise
    {
        get { return _dayInRaise;}
        set
        {
            if (value >= 0 && value < 32) _dayInRaise = value;
            else throw new ArgumentOutOfRangeException($"Неверное количество дней у поезда {this.Id} ({value})");
        }
    }
    public int RangePerDay { get; set; }
    public int RangePerMonth { get; set; }
    
    public int TransactionId { get; set; }
    public Transaction Transaction { get; set; }
    
    public int RowInFile { get; set; }
}