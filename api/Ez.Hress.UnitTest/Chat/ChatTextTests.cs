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

    [Theory]
    [InlineData("Góður", "Góður")]
    [InlineData("Fyrsta lína\nÖnnur lína", "Fyrsta lína<br />Önnur lína")]
    [InlineData("a\r\nb\rc", "a<br />b<br />c")]
    [InlineData("<img src=x onerror=alert(1)>", "&lt;img src=x onerror=alert(1)&gt;")]
    [InlineData("Malt & \"humlar\"", "Malt &amp; &quot;humlar&quot;")]
    public void ToStoredHtml_EncodesText(string text, string expected)
    {
        // ACT
        var stored = ChatText.ToStoredHtml(text);

        // ASSERT
        Assert.Equal(expected, stored);
    }

    [Theory]
    [InlineData("<b>Feitt</b>\nog <br /> líka")]
    [InlineData("Malt & humlar 'n' \"meira\"")]
    [InlineData("Þrjár\n\nlínur")]
    public void StoredHtml_RoundTripsToSameText(string text)
    {
        // ACT
        var roundTrip = ChatText.ToPlainText(ChatText.ToStoredHtml(text));

        // ASSERT
        Assert.Equal(text, roundTrip);
    }

    [Fact]
    public void Validate_TrimsText()
    {
        // ACT & ASSERT
        Assert.Equal("Góður", ChatText.Validate("  Góður\n"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \n\t ")]
    public void Validate_Blank_ThrowsArgument(string? text)
    {
        // ACT & ASSERT
        Assert.Throws<ArgumentException>(() => ChatText.Validate(text));
    }

    [Fact]
    public void Validate_MaxLength_AllowedOnlyUpToLimit()
    {
        // ACT & ASSERT
        Assert.Equal(ChatText.MaxLength, ChatText.Validate(new string('a', ChatText.MaxLength)).Length);
        Assert.Throws<ArgumentException>(() => ChatText.Validate(new string('a', ChatText.MaxLength + 1)));
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
