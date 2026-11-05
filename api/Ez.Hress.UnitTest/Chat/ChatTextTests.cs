using Ez.Hress.Chat.Entities;

namespace Ez.Hress.UnitTest.Chat;

public class ChatTextTests
{
    [Theory]
    [InlineData("Góður", "Góður")]
    [InlineData("Fyrsta lína<br />Önnur lína", "Fyrsta lína\nÖnnur lína")]
    [InlineData("a<br>b<BR/>c<br  />d", "a\nb\nc\nd")]
    [InlineData("&lt;img src=x onerror=alert(1)&gt;", "<img src=x onerror=alert(1)>")]
    [InlineData("Malt &amp; humlar", "Malt & humlar")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void ToPlainText_DecodesStoredText(string? stored, string expected)
    {
        // ACT
        var text = ChatText.ToPlainText(stored);

        // ASSERT
        Assert.Equal(expected, text);
    }

    [Fact]
    public void ToPlainText_LegacyMarkup_KeptAsLiteralText()
    {
        // ACT
        var text = ChatText.ToPlainText("<b>Feitt</b><br />næsta");

        // ASSERT
        Assert.Equal("<b>Feitt</b>\nnæsta", text);
    }
}
