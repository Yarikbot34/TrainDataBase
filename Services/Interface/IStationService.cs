using Domain.Classes;
using Domain.DTO;
namespace Services;

public interface IStationService
{
    Task<List<Station>> GetStationsAsync();
}