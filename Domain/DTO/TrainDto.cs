using System.Text.Json.Serialization;
using Domain.Classes;

namespace Domain.DTO;

public class TrainDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string Number { get; set; }
    public string Description { get; set; }
    
    //Тех. Данные
    public string StationFrom { get; set; }
    
    public string StationMiddle { get; set; }
    
    public string StationTo { get; set; }
    
    public TimeOnly TimeFrom { get; set; }
    public TimeOnly TimeTo { get; set; }
    
    public int Distance { get; set; }
    public int RailcarCount { get; set; }
    public int DayInRaise {get; set; }
    public int RangePerDay { get; set; }
    public int RangePerMonth { get; set; }
    
    public int TransactionId { get; set; }

    [JsonConstructor]
    private TrainDto() { }
    
    public TrainDto(Train train)
    {
        Id = train.Id;
        Year = train.year;
        Month = train.month;
        Number = train.Number;
        Description = train.Description;
        StationFrom = train.StationFrom.Name;
        StationMiddle = StationMiddle == null ? "" : train.StationMiddle.Name;
        StationTo = train.StationTo.Name;
        TimeFrom = train.TimeFrom;
        TimeTo = train.TimeTo;
        Distance = train.Distance;
        RailcarCount = train.RailcarCount;
        DayInRaise = train.DayInRaise;
        RangePerDay = train.RangePerDay;
        RangePerMonth = train.RangePerMonth;
        TransactionId = train.TransactionId;
    }
}

