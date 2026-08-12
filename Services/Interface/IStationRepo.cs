using Domain.Classes;
using Domain.DTO;
namespace Services;

public interface IStationRepo
{
    Task<List<Station>> GetStationsAsync();
}