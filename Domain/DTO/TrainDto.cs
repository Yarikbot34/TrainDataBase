using System.Text.Json.Serialization;
using Domain.Classes;

namespace Domain.DTO;

public class TrainDto
{
    public int Id { get; set; }
    public string Period { get; set; }
    public string Number { get; set; }
    public string Description { get; set; }
    
    //Тех. Данные
    public Station StationFrom { get; set; }
    
    public Station StationMiddle { get; set; }
    
    public Station StationTo { get; set; }
    
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
        Period = train.Period;
        Number = train.Number;
        Description = train.Description;
        StationFrom = train.StationFrom;
        StationMiddle = train.StationMiddle;
        StationTo = train.StationTo;
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

