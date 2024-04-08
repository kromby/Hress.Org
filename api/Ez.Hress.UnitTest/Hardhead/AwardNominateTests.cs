using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Hardhead
{
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
        public async Task NominateOK_Test()
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
            await interactor.Nominate(entity);
        }
        
        [Fact]
        public async Task NominateErrNoContent_Test()
        {            
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardNominateInteractor interactor = new(awardMock.Object, userMock.Object, _log.Object);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            await Assert.ThrowsAsync<ArgumentNullException>(() => interactor.Nominate(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
        
        [Fact]
        public async Task NominateErrNomineeMissing_Test()
        {
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardNominateInteractor interactor = new(awardMock.Object, userMock.Object, _log.Object);

            Nomination entity = new(1, 0, "Test")
            {
                InsertedBy = 1
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() => interactor.Nominate(entity));
        }

        [Fact]
        public async Task NominateErrTypeMissing_Test()
        {
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardNominateInteractor interactor = new(awardMock.Object, userMock.Object, _log.Object);

            Nomination entity = new(0, 1, "Test")
            {
                InsertedBy = 1
            };

            await Assert.ThrowsAsync<ArgumentException>(() => interactor.Nominate(entity));
        }

        [Fact]
        public async Task NominateErrCreatedByMissing_Test()
        {
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardNominateInteractor interactor = new(awardMock.Object, userMock.Object, _log.Object);

            Nomination entity = new(1, 1, "Test");

            await Assert.ThrowsAsync<ArgumentException>(() => interactor.Nominate(entity));
        }

        [Fact]
        public async Task NominateErrDescriptionMissing_Test()
        {
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardNominateInteractor interactor = new(awardMock.Object, userMock.Object, _log.Object);

            Nomination entity = new(1, 1, string.Empty)
            {
                InsertedBy = 1
            };

            await Assert.ThrowsAsync<ArgumentException>(() => interactor.Nominate(entity));
        }
    }
}