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

    [Fact]
    public async Task GetMessageCounts_SkipsInvalidAndDuplicateThreads()
    {
        // ARRANGE
        _dataMock.Setup(d => d.GetMessageCountsAsync(It.IsAny<IReadOnlyCollection<int>>()))
            .ReturnsAsync(new Dictionary<int, int> { [ThreadID] = 4 });
        var interactor = Arrange();

        // ACT
        var result = await interactor.GetMessageCountsAsync(new[] { ThreadID, 0, -1, ThreadID });

        // ASSERT
        Assert.Equal(4, result[ThreadID]);
        _dataMock.Verify(d => d.GetMessageCountsAsync(It.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(new[] { ThreadID }))), Times.Once);
    }

    [Fact]
    public async Task GetMessageCounts_NoThreads_DoesNotQuery()
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT
        var result = await interactor.GetMessageCountsAsync(Array.Empty<int>());

        // ASSERT
        Assert.Empty(result);
        _dataMock.Verify(d => d.GetMessageCountsAsync(It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
    }

    [Fact]
    public async Task PostMessage_StoresEncodedHtml_ReturnsPlainText()
    {
        // ARRANGE
        string? storedHtml = null;
        _dataMock.Setup(d => d.AddMessageAsync(ThreadID, 7, It.IsAny<string>(), It.IsAny<DateTime>()))
            .Callback<int, int, string, DateTime>((_, _, html, _) => storedHtml = html)
            .ReturnsAsync(99);
        _dataMock.Setup(d => d.GetMessageAsync(99)).ReturnsAsync(() =>
            new ChatMessage { ID = 99, ThreadID = ThreadID, UserID = 7, Text = storedHtml! });
        var interactor = Arrange();

        // ACT
        var result = await interactor.PostMessageAsync(ThreadID, 7, "  <img src=x onerror=alert(1)>\nGóður  ");

        // ASSERT
        Assert.Equal("&lt;img src=x onerror=alert(1)&gt;<br />Góður", storedHtml);
        Assert.Equal(99, result.ID);
        Assert.Equal("<img src=x onerror=alert(1)>\nGóður", result.Text);
    }

    [Fact]
    public async Task PostMessage_UsesUtcTime()
    {
        // ARRANGE
        DateTime? created = null;
        _dataMock.Setup(d => d.AddMessageAsync(ThreadID, 7, It.IsAny<string>(), It.IsAny<DateTime>()))
            .Callback<int, int, string, DateTime>((_, _, _, c) => created = c)
            .ReturnsAsync(99);
        _dataMock.Setup(d => d.GetMessageAsync(99)).ReturnsAsync(new ChatMessage { ID = 99 });
        var interactor = Arrange();

        // ACT
        await interactor.PostMessageAsync(ThreadID, 7, "Góður");

        // ASSERT
        Assert.Equal(DateTimeKind.Utc, created!.Value.Kind);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task PostMessage_BlankText_ThrowsArgument(string text)
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentException>(() => interactor.PostMessageAsync(ThreadID, 7, text));
        _dataMock.Verify(d => d.AddMessageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task CreateThread_TrimsName()
    {
        // ARRANGE
        _dataMock.Setup(d => d.CreateThreadAsync(77, "Tuborg Lille Jul (Jólabjór 2026)", 7, It.IsAny<DateTime>())).ReturnsAsync(ThreadID);
        var interactor = Arrange();

        // ACT
        var result = await interactor.CreateThreadAsync(77, " Tuborg Lille Jul (Jólabjór 2026) ", 7);

        // ASSERT
        Assert.Equal(ThreadID, result);
    }

    [Theory]
    [InlineData(0, "Nafn")]
    [InlineData(77, " ")]
    public async Task CreateThread_InvalidInput_ThrowsArgument(int categoryID, string name)
    {
        // ARRANGE
        var interactor = Arrange();

        // ACT & ASSERT
        await Assert.ThrowsAnyAsync<ArgumentException>(() => interactor.CreateThreadAsync(categoryID, name, 7));
        _dataMock.Verify(d => d.CreateThreadAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
    }
}
