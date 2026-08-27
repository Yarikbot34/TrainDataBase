namespace Domain.DTO;

public class RoutesWithSummDto
{
    summData SummCount {get; set;}
    summData SummPayment {get; set; }
    summData AverWayLength {get; set;}
    summData SummPaymentBySubj {get; set;}
    
    public List<RouteDto> Routes {get; set;}

    private class summData
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
            CasualSum = Routes.Sum(r => r.Casual.WayLength/r.Casual.Count),
            StudentSum = Routes.Sum(r => r.Student.WayLength/r.Student.Count),
            FedBenefitSum = Routes.Sum(r => r.FedBenefit.WayLength/r.FedBenefit.Count),
            RegBenefitSum = Routes.Sum(r => r.RegBenefit.WayLength/r.RegBenefit.Count),
            Another = Routes.Sum(r => r.Another.WayLength/r.Another.Count),
        };
        AverWayLength.FullSum = AverWayLength.CasualSum / SummCount.CasualSum +
                                AverWayLength.StudentSum / SummCount.StudentSum +
                                AverWayLength.FedBenefitSum / SummCount.FedBenefitSum +
                                AverWayLength.RegBenefitSum / SummCount.RegBenefitSum +
                                AverWayLength.Another /  SummCount.Another;
        
        
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