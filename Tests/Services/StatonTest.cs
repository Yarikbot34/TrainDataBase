using DB.Repositories;
using Domain.Classes;
using FluentAssertions;
using Moq;
using Services;

namespace Tests.Services;

public class StatonTest
{
    private readonly Mock<IStationRepo> _stationRepoMock;
    private readonly IStationService _stationService;
    
    public StatonTest()
    {
        _stationRepoMock = new Mock<IStationRepo>();
        _stationService = new StationService(_stationRepoMock.Object);
    }
    
    [Fact]
    public async Task GetAllStations()
    {
        var stations = new List<Station>()
        {
            new Station(){Id = 1, Name = "TestStation1"},
            new Station(){Id = 2, Name = "TestStation2"},
        };
        _stationRepoMock.Setup(repo => repo.GetAllStationsAsync()).ReturnsAsync(stations);

        var answ = await _stationService.GetStationsAsync();
        
        answ.Should().NotBeNull();
        answ.Count().Should().Be(2);
        
        _stationRepoMock.Verify(repo => repo.GetAllStationsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetStationsWhenNull()
    {
        var stations = new List<Station>() { };
        _stationRepoMock.Setup(repo => repo.GetAllStationsAsync()).ReturnsAsync(stations);
        
        var answ = await _stationService.GetStationsAsync();
        
        answ.Should().NotBeNull();
        answ.Count().Should().Be(0);
        
        _stationRepoMock.Verify(repo => repo.GetAllStationsAsync(), Times.Once);
    }
}