namespace Domain.Classes;

public class Route
{
    public int Id { get; set; }
    
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int RouteId => Convert.ToInt32($"{Year}{Month}{RouteNumber?.Split("/")[0]}");

    public int Month {get; set;}
    public int Year {get; set;}
    public string RouteNumber { get; set; }
    
    public List<Train> Trains { get; set; } = new();
    
    //Пассажиропоток
    public PasCategory Casual { get; set; }
    public PasCategory Student { get; set; }
    public PasCategory FedBenefit { get; set; }
    public PasCategory RegBenefit { get; set; }
    public PasCategory Another { get; set; }
    
    public int TransactionId { get; set; }
    public Transaction Transaction { get; set; }
    
    public int RowInFile { get; set; }



    public int GetPassSum()
    {
        return Casual.Count + Student.Count + FedBenefit.Count + RegBenefit.Count + Another.Count;
    }
}

public class PasCategory
{
    public int Count { get; set; }
    public double Payment { get; set; }
    public double WayLength { get; set; }
    public double PaymentBySubject { get; set; }
}
