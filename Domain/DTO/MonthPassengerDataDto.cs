namespace Domain.DTO;

public class MonthPassengerDataDto
{
    public int year { get; set; }
    public int month { get; set; }

    public int CasualCount { get; set; }
    public double CasualPercent{ get; set; }
    
    public int StudentCount { get; set; }
    public double StudentPercent { get; set; }
    
    public int FedBenefitCount{ get; set; }
    public double FedBenefitPercent{ get; set; }
    
    public int RegBenefitCount{ get; set; }
    public double RegBenefitPercent{ get; set; }
    
    public int AnotherBenefitCount{ get; set; }
    public double AnotherBenefitPercent{ get; set; }
    
    public int SumBenefitCount{ get; set; }
    public double SumBenefitPercent{ get; set; }
    
    public int SumPassengerCount{ get; set; }
}