namespace Domain.DTO;

public class TrainsWithSummDto
{
    public List<TrainDto> Trains { get; set; }
    public double Distance { get; set; }
    public double RailCarCount { get; set; }
    public double DayInRaise { get; set; }
    public double RangePerDay { get; set; }
    public double RangePerMounth { get; set; }

    public TrainsWithSummDto(List<TrainDto> trains)
    {
        Trains = trains;
        Distance = AverageIgnoringZero(trains.Select(t => t.Distance));
        RailCarCount = AverageIgnoringZero(trains.Select(t => t.RailcarCount));
        RangePerDay = AverageIgnoringZero(trains.Select(t => t.RangePerDay));

        var grouped = trains
            .Where(t => !t.IsCanceled)
            .GroupBy(t => new { t.Year, t.Month, t.Number });
        var dayValues = grouped.Select(g => g.Sum(t => t.DayInRaise));
        var monthValues = grouped.Select(g => g.Sum(t => t.RangePerMonth));
        DayInRaise = dayValues.Any() ? dayValues.Average() : 0;
        RangePerMounth = monthValues.Any() ? monthValues.Average() : 0;
    }

    private static double AverageIgnoringZero(IEnumerable<int> values)
    {
        var nonZero = values.Where(value => value != 0).ToList();
        return nonZero.Count == 0 ? 0 : nonZero.Average();
    }
}
