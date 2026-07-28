namespace DB;

public class Train
{
    public int Id { get; set; }
    public string Period { get; set; }
    public string Number { get; set; }
    
    //Тех. Данные
    public Station StationFrom { get; set; }
    public Station StationMiddle { get; set; }
    public Station StationTo { get; set; }
    
    public TimeOnly TimeFrom { get; set; }
    public TimeOnly TimeTo { get; set; }
    
    public int Distance { get; set; }
    public int RailcarCount { get; set; }
    public int DayInRaise
    {
        get { return DayInRaise;}
        set
        {
            if (value > 0 && value < 31) DayInRaise = value;
            else throw new ArgumentOutOfRangeException($"Неверное количество дней у поезда {this.Id} ({value})");
        }
    }
    public int RangePerDay { get; set; }
    public int RangePerMonth { get; set; }
    
    //Пассажиропоток
    public PasCategory Casual { get; set; }
    public PasCategory Student { get; set; }
    public PasCategory FedBenefit { get; set; }
    public PasCategory RegBenefit { get; set; }
    public PasCategory Another { get; set; }
    
    
}

public class PasCategory
{
    public int Count { get; set; }
    public int Payment { get; set; }
    public int WayLength { get; set; }
    public int PaymentBySubject { get; set; }
}
