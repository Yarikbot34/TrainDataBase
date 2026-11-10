using DB;
using DB.Repositories;
using Domain.DTO;
using Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Services;

public class SummaryDataService : ISummaryDataService
{
    private readonly IRouteRepo _routeRepo;
    private readonly ITransactionRepo _transactionRepo;
    private readonly HybridCache _cache;

    public SummaryDataService(IRouteRepo routeRepo,  ITransactionRepo transactionRepo,  HybridCache cache)
    {
        _routeRepo = routeRepo;
        _transactionRepo = transactionRepo;
        _cache = cache;  
    }

    public async Task<List<MonthPaymentDataDto>> GetYearPaymentDataInMonthAsync(int year)
    {
        var key = $"sum:Payment:{year}";
        
        var answ = await _cache.GetOrCreateAsync(
            key,
            async ct => await GetYearPaymentDataInMonth(),
            tags: new []{"summData", "routes", "data"});
        
        return answ;
        
        async Task<List<MonthPaymentDataDto>> GetYearPaymentDataInMonth()
        {
            List<MonthPaymentDataDto> payDtoList = new List<MonthPaymentDataDto>();
            
            bool isTodayYear;
            List<Route> AllRoutes;
            if (year == DateTime.Today.Year % 1000)
            {
                AllRoutes = await _routeRepo
                    .GetRoutesByYearListAsync(new List<int> { year, year - 1 }, true);
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



            foreach (var i in months)
            {
                var routes = AllRoutes.Where(r => r.Month == i && r.Year == year ||
                                                  r.Month == i && isTodayYear && r.Month > DateTime.Today.Month).ToList();
                if (routes.Count != 0)
                {
                    var transaction =
                        await _transactionRepo.GetTransactionByYearMonthTypeAsync(year, i,
                            Transaction.TransactionType.AddFile);
                    
                    if (transaction is null) continue;
                    
                    MonthPaymentDataDto dto = new MonthPaymentDataDto();
                    dto.PeriodDesc = transaction.Description;
                    dto.year = transaction.Year;
                    dto.month = transaction.Month;

                    int casualP = 0;
                    int studentP = 0;
                    int fedBenefitP = 0;
                    int regBenefitP = 0;
                    int anotherP = 0;
                    int trainKmPerMonth = 0;
                    int summ = 0;
                    foreach (var r in routes)
                    {
                        casualP += Convert.ToInt32(r.Casual.Payment + r.Casual.PaymentBySubject);
                        studentP += Convert.ToInt32(r.Student.Payment + r.Student.PaymentBySubject);
                        fedBenefitP += Convert.ToInt32(r.FedBenefit.Payment + r.FedBenefit.PaymentBySubject);
                        regBenefitP += Convert.ToInt32(r.RegBenefit.Payment + r.RegBenefit.PaymentBySubject);
                        anotherP += Convert.ToInt32(r.Another.Payment + r.Another.PaymentBySubject);
                        summ = casualP + studentP + fedBenefitP + regBenefitP + anotherP;

                        foreach (var t in r.Trains)
                        {
                            trainKmPerMonth += t.RangePerMonth;
                        }
                    }
                    
                    dto.CasualPayment = casualP;
                    dto.StudentPayment = studentP;
                    dto.RegBenefitPayment = regBenefitP;
                    dto.FedBenefitPayment = fedBenefitP;
                    dto.TrainKmPerMonth = trainKmPerMonth;
                    dto.AnotherPayment = anotherP;
                    dto.SummPayment = summ;

                    payDtoList.Add(dto);
                }
            }
            return  payDtoList;
        }
    }

    public async Task<List<MonthPassengerDataDto>> GetYearPassengerDataInMonthAsync(int year)
    {
        var key = $"sum:Passenger:{year}";

        var answ = await _cache.GetOrCreateAsync(
            key,
            async ct => await GetYearPassengerStat(),
            tags: new []{"summData", "routes", "data"});

        return answ;
        
        async Task<List<MonthPassengerDataDto>> GetYearPassengerStat()
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
            var routes = AllRoutes.Where(r => r.Month == i && r.Year == year ||
                                              r.Month == i && isTodayYear && r.Month > DateTime.Today.Month).ToList();
            if (routes.Count != 0)
            {
                var transaction =
                    await _transactionRepo.GetTransactionByYearMonthTypeAsync(year, i,
                        Transaction.TransactionType.AddFile);
                    
                if (transaction is null) continue;
                
                MonthPassengerDataDto dto = new MonthPassengerDataDto();
                dto.PeriodDesc = transaction.Description;
                dto.year = transaction.Year;
                dto.month = transaction.Month;
                
                int casualPass = 0;
                int studentPass = 0;
                int fedPass = 0;
                int regPass = 0;
                int anotherPass = 0;
                int summBenefitPass = 0;
                int summPass = 0;
                foreach (var r in routes)
                {
                    casualPass += Convert.ToInt32(r.Casual.Count);
                    studentPass += Convert.ToInt32(r.Student.Count);
                    fedPass += Convert.ToInt32(r.FedBenefit.Count);
                    regPass += Convert.ToInt32(r.RegBenefit.Count);
                    anotherPass += Convert.ToInt32(r.Another.Count);
                }
                summPass +=  casualPass + studentPass + fedPass + regPass + anotherPass;
                summBenefitPass = summPass - casualPass;

                double casualPercent = ((double)casualPass / summPass)*100;
                double studentPercent = ((double)studentPass / summPass)*100;
                double fedPercent = ((double)fedPass / summPass)*100;
                double regPercent = ((double)regPass / summPass)*100;
                double anotherPercent = ((double)anotherPass / summPass)*100;
                double summBenefitPercent = ((double)summBenefitPass / summPass)*100;
                
                dto.CasualCount = casualPass;
                dto.CasualPercent = casualPercent;
                dto.StudentCount = studentPass;
                dto.StudentPercent = studentPercent;
                dto.FedBenefitCount = fedPass;
                dto.FedBenefitPercent = fedPercent;
                dto.RegBenefitCount = regPass;
                dto.RegBenefitPercent = regPercent;
                dto.AnotherBenefitCount = anotherPass;
                dto.AnotherBenefitPercent = anotherPercent;
                dto.SumBenefitCount =  summBenefitPass;
                dto.SumBenefitPercent = summBenefitPercent;
                dto.SumPassengerCount = summPass;
                passDtoList.Add(dto);
            }
        }
        return passDtoList;
        }
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
