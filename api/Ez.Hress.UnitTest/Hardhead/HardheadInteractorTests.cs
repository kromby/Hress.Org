using System;
using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Ez.Hress.UnitTest.Hardhead;

public class HardheadInteractorTests
{
    private readonly Mock<IHardheadDataAccess> _hardheadDataAccess;
    private readonly Mock<ILogger<HardheadInteractor>> _log;
    private readonly HardheadInteractor _interactor;

    public HardheadInteractorTests()
    {
        _hardheadDataAccess = new Mock<IHardheadDataAccess>();
        _log = new Mock<ILogger<HardheadInteractor>>();
        _interactor = new HardheadInteractor(_hardheadDataAccess.Object, _log.Object);
    }

    [Fact]
    public async Task SaveRatingAsync_ExistingRating_UpdatesAndReturnsTrue()
    {
        // ARRANGE
        const int eventId = 42;
        const int userId = 7;
        const string typeCode = "REP_C_RTNG";
        const int rating = 4;

        _hardheadDataAccess
            .Setup(d => d.GetMyRatingAsync(eventId, userId))
            .ReturnsAsync(new Dictionary<string, int> { { typeCode, 3 } });

        _hardheadDataAccess
            .Setup(d => d.UpdateRatingAsync(eventId, userId, typeCode, rating))
            .ReturnsAsync(1);

        // ACT
        var result = await _interactor.SaveRatingAsync(eventId, userId, typeCode, rating);

        // ASSERT
        Assert.True(result);
        _hardheadDataAccess.Verify(d => d.UpdateRatingAsync(eventId, userId, typeCode, rating), Times.Once);
        _hardheadDataAccess.Verify(d => d.InsertRatingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        _hardheadDataAccess.Verify(d => d.GetGuests(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SaveRatingAsync_NewRating_UserAttended_InsertsAndReturnsTrue()
    {
        // ARRANGE
        const int eventId = 42;
        const int userId = 7;
        const string typeCode = "REP_C_MRTNG";
        const int rating = 5;

        _hardheadDataAccess
            .Setup(d => d.GetMyRatingAsync(eventId, userId))
            .ReturnsAsync(new Dictionary<string, int>()); // no existing rating

        _hardheadDataAccess
            .Setup(d => d.GetGuests(eventId))
            .ReturnsAsync(new List<UserBasicEntity> { new() { ID = userId } });

        _hardheadDataAccess
            .Setup(d => d.InsertRatingAsync(eventId, userId, typeCode, rating))
            .ReturnsAsync(1);

        // ACT
        var result = await _interactor.SaveRatingAsync(eventId, userId, typeCode, rating);

        // ASSERT
        Assert.True(result);
        _hardheadDataAccess.Verify(d => d.InsertRatingAsync(eventId, userId, typeCode, rating), Times.Once);
        _hardheadDataAccess.Verify(d => d.UpdateRatingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData(0, "REP_C_RTNG")]
    [InlineData(6, "REP_C_RTNG")]
    [InlineData(-1, "REP_C_RTNG")]
    [InlineData(3, "BOGUS_CODE")]
    [InlineData(3, "")]
    public async Task SaveRatingAsync_InvalidInput_ThrowsArgumentException(int rating, string typeCode)
    {
        // ACT & ASSERT
        await Assert.ThrowsAsync<System.ArgumentException>(
            () => _interactor.SaveRatingAsync(42, 7, typeCode, rating));
    }

    [Fact]
    public async Task SaveRatingAsync_NewRating_UserDidNotAttend_ReturnsFalse()
    {
        // ARRANGE
        const int eventId = 42;
        const int userId = 7;
        const string typeCode = "REP_C_RTNG";
        const int rating = 3;

        _hardheadDataAccess
            .Setup(d => d.GetMyRatingAsync(eventId, userId))
            .ReturnsAsync(new Dictionary<string, int>());

        _hardheadDataAccess
            .Setup(d => d.GetGuests(eventId))
            .ReturnsAsync(new List<UserBasicEntity>()); // user not in guest list

        // ACT
        var result = await _interactor.SaveRatingAsync(eventId, userId, typeCode, rating);

        // ASSERT
        Assert.False(result);
        _hardheadDataAccess.Verify(d => d.InsertRatingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        _hardheadDataAccess.Verify(d => d.UpdateRatingAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task RemoveGuestAsync_ValidInput_CallsDataAccess()
    {
        // ARRANGE
        const int hardheadId = 10;
        const int guestId = 5;
        const int userId = 1;

        _hardheadDataAccess
            .Setup(d => d.RemoveGuest(hardheadId, guestId))
            .ReturnsAsync(1);

        // ACT
        var result = await _interactor.RemoveGuestAsync(hardheadId, guestId, userId);

        // ASSERT
        Assert.Equal(1, result);
        _hardheadDataAccess.Verify(d => d.RemoveGuest(hardheadId, guestId), Times.Once);
    }

    [Fact]
    public async Task AddGuestAsync_GuestIsHost_ThrowsArgumentException()
    {
        // ARRANGE
        const int hardheadId = 10;
        const int hostId = 5;
        const int userId = 1;

        _hardheadDataAccess
            .Setup(d => d.GetHardhead(hardheadId))
            .ReturnsAsync(new HardheadNight(hardheadId, 1, new UserBasicEntity { ID = hostId }));

        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentException>(() => _interactor.AddGuestAsync(hardheadId, hostId, userId));
        _hardheadDataAccess.Verify(d => d.AddGuest(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }
}
