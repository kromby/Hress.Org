using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Hardhead;

public class AwardNominationTests
{
    private readonly Mock<IAwardNominationDataAccess> _nominationDataAccess;
    private readonly Mock<ILogger<AwardNominationInteractor>> _log;


    public AwardNominationTests()
    {
        _nominationDataAccess = new Mock<IAwardNominationDataAccess>();
        _log = new Mock<ILogger<AwardNominationInteractor>>();
    }

    [Fact]
    public async Task TestAsync()
    {
        // ARRANGE
        int typeID = 220;
        int userID = 2630;
        IList<Nomination> list = new List<Nomination>() { new(220, 999, "Smu"), new(220, 2640, "Smu") };

        _nominationDataAccess.Setup(x => x.GetNominations(typeID)).Returns(Task.FromResult(list));
        var awardNominationInteractor = new AwardNominationInteractor(_nominationDataAccess.Object, _log.Object);


        // ACT
        var awardNominate = await awardNominationInteractor.GetNominationsAsync(typeID, userID);

        // ASSERT
        Assert.NotNull(awardNominate);
        Assert.True(awardNominate.Count > 0);
        foreach (var nomination in awardNominate)
        {
            Assert.Equal(typeID, nomination.TypeID);
            Assert.NotEqual(userID, nomination.Nominee.ID);
        }
    }
}
