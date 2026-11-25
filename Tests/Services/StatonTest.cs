using System.ComponentModel.DataAnnotations;
using DB.Repositories;
using Domain.Classes;
using Domain.DTO;
using Domain.Enums;
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

    [Theory]
    [InlineData(StationUpdateResult.Updated)]
    [InlineData(StationUpdateResult.NotFound)]
    [InlineData(StationUpdateResult.NameConflict)]
    public async Task UpdateStationReturnsRepositoryResult(StationUpdateResult expected)
    {
        const int id = 17;
        var dto = new StationDto { Name = "Новая станция", Class = "Первый" };
        _stationRepoMock.Setup(repo => repo.UpdateStationAsync(id, dto.Name, dto.Class))
            .ReturnsAsync(expected);

        var result = await _stationService.UpdateStationAsync(id, dto);

        result.Should().Be(expected);
        _stationRepoMock.Verify(repo => repo.UpdateStationAsync(id, dto.Name, dto.Class), Times.Once);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(201, 1)]
    [InlineData(1, 51)]
    public async Task UpdateStationRejectsInvalidData(int nameLength, int classLength)
    {
        var dto = new StationDto { Name = new string('A', nameLength), Class = new string('B', classLength) };

        await Assert.ThrowsAsync<ValidationException>(() => _stationService.UpdateStationAsync(17, dto));

        _stationRepoMock.Verify(repo => repo.UpdateStationAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }
}