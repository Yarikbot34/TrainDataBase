using Domain.Classes;
using Domain.DTO;

namespace Services;

public interface IRawDataRepo
{
    Task<List<RouteDto>> getRoutesAsync(int[] years);
    Task<List<Train>> getTrainsAsync();
    Task<Train> getTrainByIdAsync(int id);
    Task PatchTrainAsync(Train train);
    Task<List<Station>> getStationsAsync();
}