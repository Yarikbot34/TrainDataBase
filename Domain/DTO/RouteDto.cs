using System.Text.Json.Serialization;
using Domain.Classes;
namespace Domain.DTO;

public class RouteDto
{
    public int Year {get;set;}
    public int Month {get;set;}
    public string RouteNumber {get;set;}
    
    public PasCategory Casual {get;set;}
    public PasCategory Student {get;set;}
    public PasCategory RegBenefit {get;set;}
    public PasCategory FedBenefit {get;set;}
    public PasCategory Another {get;set;}
    
    public PasCategory Summary {get;set;}
    
    [JsonConstructor]
    public RouteDto(){}


    public RouteDto(Route route)
    {
        Year = route.Year;
        Month = route.Month;
        RouteNumber = route.RouteNumber;
        
        Casual = route.Casual;
        Student = route.Student;
        RegBenefit = route.RegBenefit;
        FedBenefit = route.FedBenefit;
        Another = route.Another;

        var lst = new List<PasCategory>(){Casual, Student, RegBenefit, FedBenefit, Another};
        var averData = new List<double>();
        foreach (var item in lst) averData.Add(item.Count*item.WayLength);
        var aver = lst.Sum(c => c.Count) > 0 ? averData.Sum()/lst.Sum(c => c.Count): 0;
        
        Summary = new PasCategory()
        {
            Count = lst.Sum(c => c.Count),
            Payment = lst.Sum(c => c.Payment),
            PaymentBySubject = lst.Sum(c => c.PaymentBySubject),
            WayLength = aver
        };
    }
}