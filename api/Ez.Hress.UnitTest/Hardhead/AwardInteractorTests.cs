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

            Nomination entity = new(1, 1, "Test")
            {
                CreatedBy = 1                
            };

            // Þarf líka að segja í hvaða flokki er verið að tilnefna
            await interactor.Nominate(entity);
        }
        
        [Fact]
        public async void NominateErrNoContent_Test()
        {            
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardInteractor interactor = new(awardMock.Object, _log.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => interactor.Nominate(null));
        }
        
        [Fact]
        public async void NominateErrNomineeMissing_Test()
        {
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardInteractor interactor = new(awardMock.Object, _log.Object);

            Nomination entity = new(1, 0, "Test")
            {
                CreatedBy = 1
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() => interactor.Nominate(entity));
        }

        [Fact]
        public async void NominateErrTypeMissing_Test()
        {
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardInteractor interactor = new(awardMock.Object, _log.Object);

            Nomination entity = new(0, 1, "Test")
            {
                CreatedBy = 1
            };

            await Assert.ThrowsAsync<ArgumentException>(() => interactor.Nominate(entity));
        }

        [Fact]
        public async void NominateErrCreatedByMissing_Test()
        {
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardInteractor interactor = new(awardMock.Object, _log.Object);

            Nomination entity = new(1, 1, "Test");

            await Assert.ThrowsAsync<ArgumentException>(() => interactor.Nominate(entity));
        }

        [Fact]
        public async void NominateErrDescriptionMissing_Test()
        {
            awardMock.Setup(x => x.SaveNomination(It.IsAny<Nomination>()));
            AwardInteractor interactor = new(awardMock.Object, _log.Object);

            Nomination entity = new(1, 1, string.Empty)
            {
                CreatedBy = 1
            };

            await Assert.ThrowsAsync<ArgumentException>(() => interactor.Nominate(entity));
        }
    }
}