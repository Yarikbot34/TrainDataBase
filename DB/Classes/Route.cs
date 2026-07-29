namespace DB;

public class Route
{
    public int RouteId
    {
        get
        {
            return Convert.ToInt32($"{Year}{Month}{RouteNumber.Split("/")[0]}");
        }
    }
    public int Month;
    public int Year;
    public string RouteNumber { get; set; }
    public List<Train> train { get; set; }
    
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
    public double Payment { get; set; }
    public double WayLength { get; set; }
    public double PaymentBySubject { get; set; }
}
