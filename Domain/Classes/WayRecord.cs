namespace Domain.Classes;

public class WayRecord
{
    public int Id { get; set; }
    
    public int StationFromId { get; set; }
    public Station StationFrom { get; set; }
    
    public int? StationMiddleId { get; set; }
    public Station? StationMiddle { get; set; }
    
    public int StationToId { get; set; }
    public Station StationTo { get; set; }
    
    public List<int> StationIdsInWay  { get; set; } = new List<int>();
    public int wayLength { get; set; }

    public bool ContainTrain(Train train)
    {
        bool hasStations = train.StationFromId == StationFromId && train.StationToId == StationToId && train.StationMiddleId == StationMiddleId;
        bool hasSameLen = train.Distance == wayLength;
        
        return hasStations && hasSameLen;
    }
}