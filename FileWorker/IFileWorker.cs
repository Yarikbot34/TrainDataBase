using System.Security.Claims;
using Domain.Classes;
using Domain.DTO;

namespace FileWorker;

public interface IFileWorker
{
    Task<List<TrainDto>> ExtractFromFile(FileStream fs, UploadFileDto uploadDto, ClaimsPrincipal user);
}