using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.UnitTest.Hardhead
{
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
        public async void Test()
        {
            // ARRANGE
            int typeID = 220;
            int userID = 2630;
            IList<Nomination> list = new List<Nomination>() { new Nomination(220, 999, "Smu"), new Nomination(220, 2640, "Smu") };

            _nominationDataAccess.Setup(x => x.GetNominations(typeID)).Returns(Task.FromResult(list));
            var awardNominationInteractor = new AwardNominationInteractor(_nominationDataAccess.Object, _log.Object);


            // ACT
            var awardNominate = await awardNominationInteractor.GetNominations(typeID, userID);

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
}
