using Domain.Classes;

namespace Services;

public interface IRawDataRepo
{
    Task<List<Route>> getRoutesAsync(int[] years);
    Task<List<Train>> getTrainsAsync();
    Task<Train> getTrainByIdAsync(int id);
    Task PatchTrainAsync(Train train);
    Task<List<Station>> getStationsAsync();
}