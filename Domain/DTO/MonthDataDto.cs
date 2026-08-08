namespace Domain.DTO;

public class MonthDataDto
{
    public int year { get; set; }
    public int month { get; set; }
    
    public int CasualPayment { get; set; }
    public int StudentPayment { get; set; }
    public int FedBenefitPayment { get; set; }
    public int RegBenefitPayment { get; set; }
    public int AnotherPayment { get; set; }
    
    public int SummPayment { get; set; }
    
    public int TrainKmPerMonth { get; set; }
    
}