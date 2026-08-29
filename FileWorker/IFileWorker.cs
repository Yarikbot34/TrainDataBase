using System.Security.Claims;
using Domain.Classes;
using Domain.DTO;

namespace FileWorker;

public interface IFileWorker
{
    Task<List<TrainDto>> ExtractFromFile(FileStream fs, int year, int month, ClaimsPrincipal user);
}