using Ez.Hress.Chat.Entities;
using Ez.Hress.Chat.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Chat;

public class ChatInteractorTests
{
    private const int ThreadID = 1234;

    private readonly Mock<IChatDataAccess> _dataMock = new();

    private ChatInteractor Arrange() => new(_dataMock.Object, new Mock<ILogger<ChatInteractor>>().Object);

    [Fact]
    public async Task GetMessages_ReturnsPlainText_InStoredOrder()
    {
        // ARRANGE
        _dataMock.Setup(d => d.GetMessagesAsync(ThreadID)).ReturnsAsync(new List<ChatMessage>
        {
            new() { ID = 1, ThreadID = ThreadID, Text = "Fyrst&amp;fremst" },
            new() { ID = 2, ThreadID = ThreadID, Text = "a<br />b" },
        });
        var interactor = Arrange();

        // ACT
        var result = await interactor.GetMessagesAsync(ThreadID);

        // ASSERT
        Assert.Equal(new[] { 1, 2 }, result.Select(m => m.ID));
        Assert.Equal("Fyrst&fremst", result[0].Text);
        Assert.Equal("a\nb", result[1].Text);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetMessages_InvalidThread_ThrowsArgument(int threadID)
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT & ASSERT
        await Assert.ThrowsAnyAsync<ArgumentException>(() => interactor.GetMessagesAsync(threadID));
        _dataMock.Verify(d => d.GetMessagesAsync(It.IsAny<int>()), Times.Never);
    }
}
