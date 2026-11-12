using Domain.Classes;

namespace Domain.DTO;

public class StationDto
{
    public string Name { get; set; }
    public string Class { get; set; }
    
    public Station ToStation()
    {
        return new Station
        {
            Name = Name,
            Class = Class,
            InUse = false,
            IsOnMap = false
        };
    }
}

