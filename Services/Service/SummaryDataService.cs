using DB;
using DB.Repositories;
using Domain.DTO;
using Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class SummaryDataService : ISummaryDataService
{
    private readonly IRouteRepo _routeRepo;

    public SummaryDataService(IRouteRepo routeRepo)
    {
        _routeRepo = routeRepo;
    }

    public async Task<List<MonthPaymentDataDto>> GetYearPaymentDataInMonthAsync(int year)
    {
        bool isTodayYear;
        List<Route> AllRoutes;
        if (year == DateTime.Today.Year % 1000)
        {
            AllRoutes = await _routeRepo
                .GetRoutesByYearListAsync(new List<int> { year,  year - 1 }, true);
            isTodayYear = true;
        }
        else
        {
            AllRoutes = await _routeRepo
                .GetRoutesByYearListAsync(new List<int> { year }, true);
            isTodayYear = false;
        }
        
        int[] months;
        if (isTodayYear) months = getLastYearMonth();
        else months = Enumerable.Range(1, 12).ToArray();
        
        List<MonthPaymentDataDto> payDtoList = new List<MonthPaymentDataDto>();
        
        foreach (var i in months)
        {
            var routes = AllRoutes.Where(r => r.Month == i).ToList();
            if (routes.Count != 0)
            {
                MonthPaymentDataDto dto = new MonthPaymentDataDto();
                if (isTodayYear)
                {
                    dto.year = i > DateTime.Today.Month ? year - 1 : year;
                }
                else
                {
                    dto.year = year;
                }
                dto.month = i;

                int CasualP = 0;
                int StudentP = 0;
                int FedBenefitP = 0;
                int RegBenefitP = 0;
                int AnotherP = 0;
                int TrainKmPerMonth = 0;
                int Summ = 0;
                foreach (var r in routes)
                {
                    CasualP += Convert.ToInt32(r.Casual.Payment + r.Casual.PaymentBySubject);
                    StudentP += Convert.ToInt32(r.Student.Payment + r.Student.PaymentBySubject);
                    FedBenefitP += Convert.ToInt32(r.FedBenefit.Payment + r.FedBenefit.PaymentBySubject);
                    RegBenefitP += Convert.ToInt32(r.RegBenefit.Payment + r.RegBenefit.PaymentBySubject);
                    AnotherP += Convert.ToInt32(r.Another.Payment + r.Another.PaymentBySubject);
                    Summ = CasualP + StudentP + FedBenefitP + RegBenefitP + AnotherP;

                    foreach (var t in r.Trains)
                    {
                        TrainKmPerMonth += t.RangePerMonth;
                    }
                }

                dto.CasualPayment = CasualP;
                dto.StudentPayment = StudentP;
                dto.RegBenefitPayment = RegBenefitP;
                dto.FedBenefitPayment = FedBenefitP;
                dto.TrainKmPerMonth = TrainKmPerMonth;
                dto.AnotherPayment = AnotherP;
                dto.SummPayment = Summ;
                
                payDtoList.Add(dto);
            }
        }

        return payDtoList;
    }

    public async Task<List<MonthPassengerDataDto>> GetYearPassengerDataInMonthAsync(int year)
    {
        bool isTodayYear;
        List<Route> AllRoutes;
        if (year == DateTime.Today.Year % 1000)
        {
            AllRoutes = await _routeRepo
                .GetRoutesByYearListAsync(new List<int> { year,  year - 1 });
            isTodayYear = true;
        }
        else
        {
            AllRoutes = await _routeRepo
                .GetRoutesByYearListAsync(new List<int> { year });
            isTodayYear = false;
        }

        List<MonthPassengerDataDto> passDtoList = new List<MonthPassengerDataDto>();
        
        int[] months;
        if (year == DateTime.Today.Year % 1000) months = getLastYearMonth();
        else months = Enumerable.Range(1, 12).ToArray();
        
        foreach (int i in months)
        {
            var routes = AllRoutes.Where(r => r.Month == i).ToList();
            if (routes.Count != 0)
            {
                MonthPassengerDataDto dto = new MonthPassengerDataDto();
                if (isTodayYear)
                {
                    dto.year = i > DateTime.Today.Month ? year - 1 : year;
                }
                else
                {
                    dto.year = year;
                }
                dto.month = i;
                
                int CasualPass = 0;
                int StudentPass = 0;
                int FedPass = 0;
                int RegPass = 0;
                int AnotherPass = 0;
                int SummBenefitPass = 0;
                int SummPass = 0;
                foreach (var r in routes)
                {
                    CasualPass += Convert.ToInt32(r.Casual.Count);
                    StudentPass += Convert.ToInt32(r.Student.Count);
                    FedPass += Convert.ToInt32(r.FedBenefit.Count);
                    RegPass += Convert.ToInt32(r.RegBenefit.Count);
                    AnotherPass += Convert.ToInt32(r.Another.Count);
                }
                SummPass +=  CasualPass + StudentPass + FedPass + RegPass + AnotherPass;
                SummBenefitPass = SummPass - CasualPass;

                double CasualPercent = ((double)CasualPass / SummPass)*100;
                double StudentPercent = ((double)StudentPass / SummPass)*100;
                double FedPercent = ((double)FedPass / SummPass)*100;
                double RegPercent = ((double)RegPass / SummPass)*100;
                double AnotherPercent = ((double)AnotherPass / SummPass)*100;
                double SummBenefitPercent = ((double)SummBenefitPass / SummPass)*100;
                
                dto.CasualCount = CasualPass;
                dto.CasualPercent = CasualPercent;
                dto.StudentCount = StudentPass;
                dto.StudentPercent = StudentPercent;
                dto.FedBenefitCount = FedPass;
                dto.FedBenefitPercent = FedPercent;
                dto.RegBenefitCount = RegPass;
                dto.RegBenefitPercent = RegPercent;
                dto.AnotherBenefitCount = AnotherPass;
                dto.AnotherBenefitPercent = AnotherPercent;
                dto.SumBenefitCount =  SummBenefitPass;
                dto.SumBenefitPercent = SummBenefitPercent;
                dto.SumPassengerCount = SummPass;
                passDtoList.Add(dto);
            }
        }
        return passDtoList;
    }

    int[] getLastYearMonth()
    {
        var today = DateTime.Today;
        List<int> months = Enumerable.Range(0, 12)
            .Select(i => today.AddMonths(i - 11).Month)
            .ToList();
        return months.ToArray();
    }
}
