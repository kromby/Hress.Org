using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Hardhead;

public class PostElectionInteractorTests
{
    private readonly Mock<IRuleDataAccess> _ruleDataAccessMock;
    private readonly Mock<IRuleChangeDataAccess> _ruleChangeDataAccessMock;
    private readonly Mock<ITypeInteractor> _typeInteractorMock;
    private readonly Mock<ILogger<RuleInteractor>> _ruleInteractorLogMock;
    private readonly Mock<IElectionVoteDataAccess> _voteDataAccessMock;

    public PostElectionInteractorTests()
    {
        _ruleDataAccessMock = new Mock<IRuleDataAccess>();
        _ruleChangeDataAccessMock = new Mock<IRuleChangeDataAccess>(MockBehavior.Strict);
        _typeInteractorMock = new Mock<ITypeInteractor>();
        _ruleInteractorLogMock = new Mock<ILogger<RuleInteractor>>();
        _voteDataAccessMock = new Mock<IElectionVoteDataAccess>();
    }

    [Fact]
    public async Task UpdateRulesOK_TestAsync()
    {
        // Arrange
        var guidVoteZero = Guid.NewGuid();
        IList<RuleChange> list = new List<RuleChange>()
        {
            new RuleChange(RuleChangeType.Create, 23532, "Hinrik hefur í fortíðinni" ),
            //new RuleChange(RuleChangeType.Delete, 21707, "Nóg að hafa þetta undir klæðnaður"),
            //new RuleChange(RuleChangeType.Update, 21717, "Óþarfi því í reglu 5.6"),
        };
        list[0].ID = guidVoteZero.ToString();
        //list[1].ID = Guid.NewGuid().ToString();
        //list[2].ID = Guid.NewGuid().ToString();
        _ruleChangeDataAccessMock.Setup(rc => rc.GetRuleChanges()).Returns(Task.FromResult(list));
        _typeInteractorMock.Setup(t => t.GetEzType((int)RuleChangeType.Create)).Returns(Task.FromResult(new Hress.Shared.Entities.TypeEntity((int)RuleChangeType.Create, "Create Unit test", "UNIT-TEST")));
        _typeInteractorMock.Setup(t => t.GetEzType((int)RuleChangeType.Update)).Returns(Task.FromResult(new Hress.Shared.Entities.TypeEntity((int)RuleChangeType.Update, "Update Unit test", "UNIT-TEST")));
        _typeInteractorMock.Setup(t => t.GetEzType((int)RuleChangeType.Delete)).Returns(Task.FromResult(new Hress.Shared.Entities.TypeEntity((int)RuleChangeType.Delete, "Delete Unit test", "UNIT-TEST")));
        var ruleInteractor = new RuleInteractor(_ruleDataAccessMock.Object, _ruleChangeDataAccessMock.Object, _typeInteractorMock.Object, _ruleInteractorLogMock.Object);
        
        IList<VoteEntity> votes = new List<VoteEntity>()
        {
            new VoteEntity(guidVoteZero, 100, "1"),
            new VoteEntity(guidVoteZero, 100, "1"),
            new VoteEntity(guidVoteZero, 100, "1"),
            new VoteEntity(guidVoteZero, 100, "1"),
            new VoteEntity(guidVoteZero, 100, "-1"),
        };
        _voteDataAccessMock.Setup(t => t.GetVotes(guidVoteZero)).Returns(Task.FromResult(votes));
        var postElectionInteractor = new PostElectionInteractor(ruleInteractor, _voteDataAccessMock.Object);

        // Act
        string updatedRules = await postElectionInteractor.UpdateRules();

        // Assert
        Assert.NotNull(updatedRules);
        Assert.True(updatedRules.Length > 0);
    }
}
