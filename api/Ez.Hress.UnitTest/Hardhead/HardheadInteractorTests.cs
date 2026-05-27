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
}
