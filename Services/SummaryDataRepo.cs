using DB;
using Domain.DTO;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class SummaryDataRepo : ISummaryDataRepo
{
    private readonly AppDbContext ldb;

    public SummaryDataRepo(AppDbContext db)
    {
        ldb = db;
    }
    
    public async Task<List<MonthDataDto>> GetYearDataAsync(int year)
    {
        var AllRoutes = ldb.Routes.Where(r => r.Year == year).Include(r => r.Trains).ToList();
        List <MonthDataDto> dtoList = new List<MonthDataDto>();
        for (int i = 1; i < DateTime.Today.Month; i++)
        {
            var routes = AllRoutes.Where(r => r.Month == i).ToList();
            if (routes.Count != 0)
            {
                MonthDataDto dto = new MonthDataDto();
                dto.year = year;
                dto.month = i;

                int CasualP = 0;
                int StudentP = 0;
                int FedBenefitP = 0;
                int RegBenefitP = 0;
                int AnotherP = 0;
                int TrainKmPerMonth = 0;
                foreach (var r in routes)
                {
                    CasualP += Convert.ToInt32(r.Casual.Payment);
                    StudentP += Convert.ToInt32(r.Student.Payment);
                    FedBenefitP += Convert.ToInt32(r.FedBenefit.Payment);
                    RegBenefitP += Convert.ToInt32(r.RegBenefit.Payment);
                    
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
                dtoList.Add(dto);
            }
        }
        return dtoList;
    }
}