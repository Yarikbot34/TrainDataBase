namespace Domain.DTO;

public class MonthPassengerDataDto
{
    public int year { get; set; }
    public int month { get; set; }

    public int CasualCount;
    public double CasualPercent;
    
    public int StudentCount;
    public double StudentPercent;
    
    public int FedBenefitCount;
    public double FedBenefitPercent;
    
    public int RegBenefitCount;
    public double RegBenefitPercent;
    
    public int AnotherBenefitCount;
    public double AnotherBenefitPercent;
    
    public int SumBenefitCount;
    public double SumBenefitPercent;
    
    public int SumPassengerCount;
}