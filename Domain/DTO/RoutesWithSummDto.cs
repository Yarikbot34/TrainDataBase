namespace Domain.DTO;

public class RoutesWithSummDto
{
    public summData SummCount {get; set;}
    public summData SummPayment {get; set; }
    public summData AverWayLength {get; set;}
    public summData SummPaymentBySubj {get; set;}
    
    public List<RouteDto> Routes {get; set;}

    public class summData
    {
        public double FullSum {get; set;}
        public double CasualSum{get; set;}
        public double StudentSum{get; set;}
        public double FedBenefitSum{get; set;}
        public double RegBenefitSum{get; set;}
        public double Another{get; set;}

        public double GetSum()
        {
            return CasualSum + StudentSum + FedBenefitSum + RegBenefitSum + Another;
        }
    }
    

    public RoutesWithSummDto(List<RouteDto> routes)
    {
        Routes = routes;
        SummCount = new summData
        {
                CasualSum = Routes.Sum(r => r.Casual.Count),
                StudentSum = Routes.Sum(r => r.Student.Count),
                FedBenefitSum = Routes.Sum(r => r.FedBenefit.Count),
                RegBenefitSum = Routes.Sum(r => r.RegBenefit.Count),
                Another = Routes.Sum(r => r.Another.Count),
        };
        SummCount.FullSum = SummCount.GetSum();
        
        SummPayment = new summData
        {
            CasualSum = Routes.Sum(r => r.Casual.Payment),
            StudentSum = Routes.Sum(r => r.Student.Payment),
            FedBenefitSum = Routes.Sum(r => r.FedBenefit.Payment),
            RegBenefitSum = Routes.Sum(r => r.RegBenefit.Payment),
            Another = Routes.Sum(r => r.Another.Payment),
        };
        SummPayment.FullSum = SummPayment.GetSum();
        
        AverWayLength = new summData
        {
            CasualSum = SummCount.CasualSum == 0 ? 0 
                : Routes.Sum(r => r.Casual.WayLength * r.Casual.Count) / Routes.Sum(r => r.Casual.Count),

            StudentSum = SummCount.StudentSum == 0 ? 0 
                : Routes.Sum(r => r.Student.WayLength * r.Student.Count) / Routes.Sum(r => r.Student.Count),

            FedBenefitSum = SummCount.FedBenefitSum == 0 ? 0 
                : Routes.Sum(r => r.FedBenefit.WayLength * r.FedBenefit.Count) / Routes.Sum(r => r.FedBenefit.Count),

            RegBenefitSum = SummCount.RegBenefitSum == 0 ? 0 
                : Routes.Sum(r => r.RegBenefit.WayLength * r.RegBenefit.Count) / Routes.Sum(r => r.RegBenefit.Count),

            Another = SummCount.Another == 0 ? 0 
                : Routes.Sum(r => r.Another.WayLength * r.Another.Count) / Routes.Sum(r => r.Another.Count),
        };
        double totalLen = AverWayLength.CasualSum * SummCount.CasualSum +
                          AverWayLength.StudentSum * SummCount.StudentSum +
                          AverWayLength.FedBenefitSum * SummCount.FedBenefitSum +
                          AverWayLength.RegBenefitSum * SummCount.RegBenefitSum +
                          AverWayLength.Another * SummCount.Another;
        
        AverWayLength.FullSum = SummCount.FullSum == 0 ? 0 : totalLen/SummCount.FullSum;
        
        SummPaymentBySubj = new summData
        {
            CasualSum = Routes.Sum(r => r.Casual.Payment),
            StudentSum = Routes.Sum(r => r.Student.Payment),
            FedBenefitSum = Routes.Sum(r => r.FedBenefit.Payment),
            RegBenefitSum = Routes.Sum(r => r.RegBenefit.Payment),
            Another = Routes.Sum(r => r.Another.Payment)
        };
        SummPaymentBySubj.FullSum = SummPaymentBySubj.GetSum();
    }
}