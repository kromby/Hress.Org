using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Hardhead;

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
    public async Task GetRuleChangesOK_TestAsync()
    {
        // ARRANGE
        IList<RuleChange> list = new List<RuleChange>()
        {
            new RuleChange(RuleChangeType.Create, 23532, "Hinrik hefur í fortíðinni" ),
            new RuleChange(RuleChangeType.Delete, 21707, "Nóg að hafa þetta undir klæðnaður"),
            new RuleChange(RuleChangeType.Update, 21717, "Óþarfi því í reglu 5.6"),
        };
        _ruleChangeDataAccessMock.Setup(rc => rc.GetRuleChanges()).Returns(Task.FromResult(list));
        int typeID = 209;
        _typeInteractorMock.Setup(t => t.GetEzType(typeID)).Returns(Task.FromResult(new Hress.Shared.Entities.TypeEntity(typeID, "Unit test", "UNIT-TEST")));
        RuleInteractor interactor = new(_ruleDataAccessMock.Object, _ruleChangeDataAccessMock.Object, _typeInteractorMock.Object, _ruleInteractorLogMock.Object);
        

        // ACT
        var result = await interactor.GetRuleChangesAsync(typeID);

        // ASSERT
        Assert.NotNull(result);
        Assert.Single(result);
        _ruleChangeDataAccessMock.Verify();
    }
}
