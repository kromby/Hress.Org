using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Hardhead
{
    public class AwardInteractorTests
    {
        private readonly Mock<IAwardDataAccess> awardMock;
        private readonly Mock<ILogger<AwardInteractor>> _log;
        public AwardInteractorTests()
        {
            awardMock = new Mock<IAwardDataAccess>();
            _log = new Mock<ILogger<AwardInteractor>>();
        }
        
        [Fact]
        public async void NominateOK_Test()
        {
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardInteractor interactor = new(awardMock.Object, _log.Object);

            Nomination entity = new()
            {
                NomineeID = 1,
                TypeID = 1,
                CreatedBy = 1,
                Description = "Test"
            };

            // Þarf líka að segja í hvaða flokki er verið að tilnefna
            await interactor.Nominate(entity);
        }
        
        [Fact]
        public void NominateErrNoContent_Test()
        {            
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardInteractor interactor = new(awardMock.Object, _log.Object);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.ThrowsAsync<ArgumentNullException>(() => interactor.Nominate(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
        
        [Fact]
        public void NominateErrNomineeMissing_Test()
        {
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardInteractor interactor = new(awardMock.Object, _log.Object);

            Nomination entity = new()
            {
                TypeID = 1,
                CreatedBy = 1,
                Description = "Test"
            };

            Assert.ThrowsAsync<ArgumentException>(() => interactor.Nominate(entity));
        }

        [Fact]
        public void NominateErrTypeMissing_Test()
        {
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardInteractor interactor = new(awardMock.Object, _log.Object);

            Nomination entity = new()
            {
                NomineeID = 1,
                CreatedBy = 1,
                Description = "Test"
            };

            Assert.ThrowsAsync<ArgumentException>(() => interactor.Nominate(entity));
        }

        [Fact]
        public void NominateErrCreatedByMissing_Test()
        {
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardInteractor interactor = new(awardMock.Object, _log.Object);

            Nomination entity = new()
            {
                NomineeID = 1,
                TypeID = 1,
                Description = "Test"
            };

            Assert.ThrowsAsync<ArgumentException>(() => interactor.Nominate(entity));
        }

        [Fact]
        public void NominateErrDescriptionMissing_Test()
        {
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardInteractor interactor = new(awardMock.Object, _log.Object);

            Nomination entity = new()
            {
                NomineeID = 1,
                TypeID = 1,
                CreatedBy = 1,
                Description = String.Empty
            };

            Assert.ThrowsAsync<ArgumentException>(() => interactor.Nominate(entity));
        }
    }
}