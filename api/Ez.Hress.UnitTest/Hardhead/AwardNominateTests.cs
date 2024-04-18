using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Hardhead;

public class AwardNominateTests
{
    private readonly Mock<IAwardNominateDataAccess> awardMock;
    private readonly Mock<IUserInteractor> userMock;
    private readonly Mock<ILogger<AwardNominateInteractor>> _log;
    public AwardNominateTests()
    {
        awardMock = new Mock<IAwardNominateDataAccess>();
        userMock = new Mock<IUserInteractor>();
        _log = new Mock<ILogger<AwardNominateInteractor>>();
    }
    
    [Fact]
    public async Task NominateOK_TestAsync()
    {
        // SETUP
        awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
        userMock.Setup(y => y.GetUser(It.IsAny<int>())).ReturnsAsync(new UserBasicEntity { ID = 1, Username = "test" });
        AwardNominateInteractor interactor = new(awardMock.Object, userMock.Object, _log.Object);

        Nomination entity = new(1, 1, "Test")
        {
            InsertedBy = 1                
        };

        // ACT
        await interactor.NominateAsync(entity);
    }
    
    [Fact]
    public async Task NominateErrNoContent_TestAsync()
    {            
        awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
        AwardNominateInteractor interactor = new(awardMock.Object, userMock.Object, _log.Object);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        await Assert.ThrowsAsync<ArgumentNullException>(() => interactor.NominateAsync(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
    
    [Fact]
    public async Task NominateErrNomineeMissing_TestAsync()
    {
        awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
        AwardNominateInteractor interactor = new(awardMock.Object, userMock.Object, _log.Object);

        Nomination entity = new(1, 0, "Test")
        {
            InsertedBy = 1
        };

        await Assert.ThrowsAsync<ArgumentNullException>(() => interactor.NominateAsync(entity));
    }

    [Fact]
    public async Task NominateErrTypeMissing_TestAsync()
    {
        awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
        AwardNominateInteractor interactor = new(awardMock.Object, userMock.Object, _log.Object);

        Nomination entity = new(0, 1, "Test")
        {
            InsertedBy = 1
        };

        await Assert.ThrowsAsync<ArgumentException>(() => interactor.NominateAsync(entity));
    }

    [Fact]
    public async Task NominateErrCreatedByMissing_TestAsync()
    {
        awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
        AwardNominateInteractor interactor = new(awardMock.Object, userMock.Object, _log.Object);

        Nomination entity = new(1, 1, "Test");

        await Assert.ThrowsAsync<ArgumentException>(() => interactor.NominateAsync(entity));
    }

    [Fact]
    public async Task NominateErrDescriptionMissing_TestAsync()
    {
        awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
        AwardNominateInteractor interactor = new(awardMock.Object, userMock.Object, _log.Object);

        Nomination entity = new(1, 1, string.Empty)
        {
            InsertedBy = 1
        };

        await Assert.ThrowsAsync<ArgumentException>(() => interactor.NominateAsync(entity));
    }
}