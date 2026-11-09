using System.Reflection;
using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Localization;

namespace Laqeeta.Tests;

public class LocalizationTests
{
    private static IEnumerable<string> Constants(Type type)
        => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
               .Where(f => f.IsLiteral && f.FieldType == typeof(string))
               .Select(f => (string)f.GetRawConstantValue()!);

    [Fact]
    public void Every_error_code_has_an_arabic_and_an_english_text()
    {
        var codes = Constants(typeof(Err)).ToList();
        Assert.NotEmpty(codes);

        foreach (var code in codes)
        {
            Assert.True(Messages.ErrorTable.ContainsKey(code), $"missing translation for {code}");
            var (ar, en) = Messages.ErrorTable[code];
            Assert.False(string.IsNullOrWhiteSpace(ar), $"empty Arabic text for {code}");
            Assert.False(string.IsNullOrWhiteSpace(en), $"empty English text for {code}");
            Assert.NotEqual(ar, en);
        }
    }

    [Fact]
    public void The_translation_table_has_no_unused_entries()
    {
        var codes = Constants(typeof(Err)).ToHashSet();
        Assert.Empty(Messages.ErrorTable.Keys.Where(k => !codes.Contains(k)));
    }

    [Fact]
    public void Every_notification_code_has_all_four_texts()
    {
        foreach (var code in Constants(typeof(NotifCode)))
        {
            Assert.True(Messages.NotificationTable.ContainsKey(code), $"missing translation for {code}");
            var t = Messages.NotificationTable[code];
            Assert.False(string.IsNullOrWhiteSpace(t.TitleAr));
            Assert.False(string.IsNullOrWhiteSpace(t.TitleEn));
            Assert.False(string.IsNullOrWhiteSpace(t.BodyAr));
            Assert.False(string.IsNullOrWhiteSpace(t.BodyEn));
        }
    }

    [Fact]
    public void Error_uses_the_requested_language_and_falls_back_to_the_message_without_a_code()
    {
        var withCode = new ForbiddenException("English fallback", Err.WrongStation);

        Assert.Equal("The item is held at a different station.", Messages.Error(withCode, "en"));
        Assert.Equal(Messages.ErrorTable[Err.WrongStation].Ar, Messages.Error(withCode, "ar"));

        var withoutCode = new BusinessRuleException("Only English here");
        Assert.Equal("Only English here", Messages.Error(withoutCode, "ar"));

        var unknownCode = new BusinessRuleException("fallback", "err.doesNotExist");
        Assert.Equal("fallback", Messages.Error(unknownCode, "ar"));
    }

    [Theory]
    [InlineData("ar", "ar")]
    [InlineData("ar-EG,ar;q=0.9,en;q=0.8", "ar")]
    [InlineData("AR", "ar")]
    [InlineData("en-US,en;q=0.9,ar;q=0.8", "en")]
    [InlineData("fr", "en")]
    [InlineData("", "en")]
    [InlineData(null, "en")]
    public void Normalize_picks_arabic_only_when_it_is_the_first_choice(string? header, string expected)
        => Assert.Equal(expected, Messages.Normalize(header));

    [Fact]
    public void Notification_text_is_translated_and_the_parameter_fills_the_placeholder()
    {
        var ar = Messages.Body(NotifCode.MatchConfirmed, "stored", "Sadat", "ar");
        var en = Messages.Body(NotifCode.MatchConfirmed, "stored", "Sadat", "en");

        Assert.Equal(Messages.NotificationTable[NotifCode.MatchConfirmed].BodyAr.Replace("{0}", "Sadat"), ar);
        Assert.Equal("Please visit Sadat with your ID to claim it.", en);

        // no station name => a generic word instead of an empty hole
        Assert.Equal("Please visit the station with your ID to claim it.", Messages.Body(NotifCode.MatchConfirmed, "stored", null, "en"));
        Assert.DoesNotContain("{0}", Messages.Body(NotifCode.MatchConfirmed, "stored", null, "ar"));
    }

    [Fact]
    public void Old_notifications_without_a_code_are_shown_as_stored()
    {
        Assert.Equal("Old title", Messages.Title(null, "Old title", "ar"));
        Assert.Equal("Old message", Messages.Body(null, "Old message", null, "ar"));
        Assert.Equal("Old title", Messages.Title("notif.unknown", "Old title", "ar"));
    }
}
