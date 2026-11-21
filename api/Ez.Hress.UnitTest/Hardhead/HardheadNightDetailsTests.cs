using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Hardhead;

/// <summary>Location and start time of a Hardhead night.</summary>
public class HardheadNightDetailsTests
{
    private readonly Mock<IHardheadDataAccess> _dataAccess = new();
    private readonly HardheadInteractor _sut;
    private HardheadNight? _altered;

    private static readonly DateTime StoredDate = new(2026, 11, 30);

    public HardheadNightDetailsTests()
    {
        _dataAccess.Setup(d => d.AlterHardhead(It.IsAny<HardheadNight>()))
            .Callback<HardheadNight>(n => _altered = n)
            .ReturnsAsync(true);
        SetStored(location: null);
        _sut = new HardheadInteractor(_dataAccess.Object, Mock.Of<ILogger<HardheadInteractor>>());
    }

    private void SetStored(string? location) =>
        _dataAccess.Setup(d => d.GetHardhead(123)).ReturnsAsync(Night(StoredDate, location));

    private static HardheadNight Night(DateTime date, string? location = null) =>
        new(123, 250, new UserBasicEntity { ID = 3, Username = "Jónsi" }) { Date = date, Location = location };

    [Fact]
    public async Task Save_WithLocation_SavesItTrimmed()
    {
        await _sut.SaveHardheadAsync(Night(new DateTime(2026, 11, 20, 20, 0, 0), "  Bláskógar 3 "), 9);

        _dataAccess.Verify(d => d.SaveLocation(123, "Bláskógar 3", 9, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task Save_LocationNull_LeavesItAlone()
    {
        SetStored("Bláskógar 3");

        await _sut.SaveHardheadAsync(Night(new DateTime(2026, 11, 20), location: null), 9);

        _dataAccess.Verify(d => d.SaveLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Save_LocationUnchanged_NotSavedAgain()
    {
        SetStored("Bláskógar 3");

        await _sut.SaveHardheadAsync(Night(new DateTime(2026, 11, 20), "Bláskógar 3"), 9);

        _dataAccess.Verify(d => d.SaveLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Save_LocationEmpty_ClearsIt(string location)
    {
        SetStored("Bláskógar 3");

        await _sut.SaveHardheadAsync(Night(new DateTime(2026, 11, 20), location), 9);

        _dataAccess.Verify(d => d.SaveLocation(123, "", 9, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task Save_NoLocationBeforeOrAfter_NothingSaved()
    {
        await _sut.SaveHardheadAsync(Night(new DateTime(2026, 11, 20), ""), 9);

        _dataAccess.Verify(d => d.SaveLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Save_LocationTooLong_ThrowsBeforeSavingAnything()
    {
        var night = Night(new DateTime(2026, 11, 20), new string('a', HardheadNight.LocationMaxLength + 1));

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.SaveHardheadAsync(night, 9));

        _dataAccess.Verify(d => d.AlterHardhead(It.IsAny<HardheadNight>()), Times.Never);
        _dataAccess.Verify(d => d.SaveLocation(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task Save_StartTime_KeptToTheMinute()
    {
        await _sut.SaveHardheadAsync(Night(new DateTime(2026, 11, 20, 20, 30, 45, 123)), 9);

        Assert.Equal(new DateTime(2026, 11, 20, 20, 30, 0), _altered!.Date);
    }

    [Fact]
    public void StartTime_Midnight_IsNotSet()
    {
        var night = Night(new DateTime(2026, 11, 20));

        Assert.Null(night.StartTime);
        Assert.Null(night.StartTimeString);
    }

    [Fact]
    public void StartTime_FromTimeOfDay()
    {
        var night = Night(new DateTime(2026, 11, 20, 20, 0, 0));

        Assert.Equal(new TimeSpan(20, 0, 0), night.StartTime);
        Assert.Equal("20:00", night.StartTimeString);
    }

    [Fact]
    public void DateString_FutureNightWithoutTime_ShowsOnlyMonth()
    {
        var night = Night(new DateTime(DateTime.UtcNow.Year + 1, 3, 14));

        Assert.Equal($"mars {DateTime.UtcNow.Year + 1}", night.DateString);
    }

    [Fact]
    public void DateString_FutureNightWithTime_ShowsDay()
    {
        var night = Night(new DateTime(DateTime.UtcNow.Year + 1, 3, 14, 20, 0, 0));

        Assert.Equal($"14. mars {DateTime.UtcNow.Year + 1}", night.DateString);
    }
}
