using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.UnitTest.Hardhead
{
    public class RuleInteractorTests
    {
        private readonly Mock<IRuleDataAccess> _ruleDataAccessMock;
        private readonly Mock<IRuleChangeDataAccess> _ruleChangeDataAccessMock;
        private readonly Mock<ITypeInteractor> _typeInteractorMock;
        private readonly Mock<ILogger<RuleInteractor>> _ruleInteractorLogMock;

        public RuleInteractorTests()
        {
            _ruleDataAccessMock = new Mock<IRuleDataAccess>();
            _ruleChangeDataAccessMock = new Mock<IRuleChangeDataAccess>(MockBehavior.Strict);
            _typeInteractorMock = new Mock<ITypeInteractor>();
            _ruleInteractorLogMock = new Mock<ILogger<RuleInteractor>>();
        }

        [Fact]
        public async void GetRuleChangesOK_Test()
        {
            // ARRANGE
            IList<RuleChange> list = new List<RuleChange>()
            {
                new RuleChange(RuleChangeType.Create, 23532, "Hinrik hefur í fortíðinni" ),
                new RuleChange(RuleChangeType.Delete, 21707, "Nóg að hafa þetta undir klæðnaður"),
                new RuleChange(RuleChangeType.Update, 21717, "Óþarfi því í reglu 5.6"),
            };
            _ruleChangeDataAccessMock.Setup(rc => rc.GetRuleChanges()).Returns(Task.FromResult(list));
            RuleInteractor interactor = new(_ruleDataAccessMock.Object, _ruleChangeDataAccessMock.Object, _typeInteractorMock.Object, _ruleInteractorLogMock.Object);
            int typeID = 209;

            // ACT
            var result = await interactor.GetRuleChanges(typeID);

            // ASSERT
            Assert.NotNull(result);
            Assert.Single(result);
            _ruleChangeDataAccessMock.Verify();
        }
    }
}
