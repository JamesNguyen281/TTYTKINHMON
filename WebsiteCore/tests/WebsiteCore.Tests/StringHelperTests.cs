using WebsiteCore.Business.Helpers;
using Xunit;

namespace WebsiteCore.Tests;

public class StringHelperTests
{
    [Fact]
    public void ChangeText_RemovesVietnameseDiacritics()
    {
        var slug = StringHelper.ChangeText("Khoa Y học cổ truyền và Phục hồi chức năng");
        Assert.Equal("khoa-y-hoc-co-truyen-va-phuc-hoi-chuc-nang", slug);
    }

    [Fact]
    public void ChangeText_HandlesDD()
    {
        Assert.Equal("dat-lich-kham", StringHelper.ChangeText("Đặt lịch khám"));
        Assert.Equal("don-vi-do", StringHelper.ChangeText("Đơn vị Đo"));
    }

    [Fact]
    public void ChangeText_EmptyOrNull()
    {
        Assert.Equal(string.Empty, StringHelper.ChangeText(""));
        Assert.Equal(string.Empty, StringHelper.ChangeText(null!));
    }

    [Fact]
    public void HashPassword_ProducesPbkdf2Format()
    {
        // OWASP 2024: PBKDF2-SHA256 ≥ 600k iterations
        var hash = StringHelper.HashPassword("Test1234");
        Assert.StartsWith("pbkdf2$600000$", hash);
        var parts = hash.Split('$');
        Assert.Equal(4, parts.Length);
    }

    [Fact]
    public void HashPassword_ProducesUniqueSalt()
    {
        var h1 = StringHelper.HashPassword("same");
        var h2 = StringHelper.HashPassword("same");
        Assert.NotEqual(h1, h2);
    }

    [Fact]
    public void VerifyPassword_PbkdfRoundtrip()
    {
        var hash = StringHelper.HashPassword("MySecret123");
        Assert.True(StringHelper.VerifyPassword("MySecret123", hash));
        Assert.False(StringHelper.VerifyPassword("MySecret124", hash));
        Assert.False(StringHelper.VerifyPassword("", hash));
    }

    [Fact]
    public void VerifyPassword_Md5LegacyFallback()
    {
        // MD5("123456") = e10adc3949ba59abbe56e057f20f883e
        var stored = "e10adc3949ba59abbe56e057f20f883e";
        Assert.True(StringHelper.VerifyPassword("123456", stored));
        Assert.True(StringHelper.VerifyPassword("123456", stored.ToUpper())); // case-insensitive
        Assert.False(StringHelper.VerifyPassword("wrong", stored));
    }

    [Fact]
    public void VerifyPassword_NullStoredReturnsFalse()
    {
        Assert.False(StringHelper.VerifyPassword("anything", null));
        Assert.False(StringHelper.VerifyPassword("anything", ""));
    }

    [Fact]
    public void VerifyPassword_RejectsUnknownFormat()
    {
        // Old ASP.NET Identity hash starts with AQAA — KHÔNG support, phải fail an toàn
        var oldFmt = "AQAAAAEAACcQAAAAEH9pUM2YT9MnNK0sPvPZRQ7tDR3MIz2N+KH+JPKaNFFZbNOdDB8YOKa+aJbxUcK7pA==";
        Assert.False(StringHelper.VerifyPassword("123456", oldFmt));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Short1!")]      // 7 ký tự (<8)
    [InlineData("alllower1!")]   // ko HOA
    [InlineData("ALLUPPER1!")]   // ko thường
    [InlineData("NoDigits!")]    // ko số
    [InlineData("NoSpecial1")]   // ko special
    [InlineData("123456")]       // banned pattern + ko đủ tiêu chuẩn
    [InlineData("Password1!")]   // banned pattern (password)
    [InlineData("Admin1234!")]   // banned pattern (admin)
    [InlineData("aaaa1A!a")]     // 4 ký tự lặp liên tiếp
    public void ValidatePasswordStrength_Rejects_Weak(string? input)
    {
        Assert.NotNull(StringHelper.ValidatePasswordStrength(input));
    }

    [Theory]
    [InlineData("Strong1!Pwd")]
    [InlineData("MyP@ssw0rd")] // 'password' substring → check this matters
    [InlineData("XyZ12@aB!c")]
    public void ValidatePasswordStrength_Accepts_Strong(string input)
    {
        // Note: "MyP@ssw0rd" contains "ssw0rd" but lowercased = "myp@ssw0rd" — does NOT contain "password" so pass
        var err = StringHelper.ValidatePasswordStrength(input);
        Assert.True(err == null, $"Expected null, got: {err}");
    }

    [Fact]
    public void ValidatePasswordStrength_RejectsTooLong()
    {
        var pwd = new string('a', 101) + "B1!";
        Assert.NotNull(StringHelper.ValidatePasswordStrength(pwd));
    }

    [Fact]
    public void IsDefaultPassword_DetectsMd5Of123456()
    {
        // MD5("123456") = e10adc3949ba59abbe56e057f20f883e
        Assert.True(StringHelper.IsDefaultPassword("e10adc3949ba59abbe56e057f20f883e"));
        Assert.False(StringHelper.IsDefaultPassword(StringHelper.HashPassword("Strong1!")));
        Assert.False(StringHelper.IsDefaultPassword(null));
        Assert.False(StringHelper.IsDefaultPassword(""));
    }

    [Fact]
    public void IsDefaultPassword_DetectsPbkdf2Of123456()
    {
        var hash = StringHelper.HashPassword("123456");
        Assert.True(StringHelper.IsDefaultPassword(hash));
    }

    [Fact]
    public void SubString_TruncatesAndAppendsEllipsis()
    {
        Assert.Equal("Hello...", StringHelper.SubString("HelloWorld", 5));
        Assert.Equal("Short", StringHelper.SubString("Short", 10));
        Assert.Equal(string.Empty, StringHelper.SubString(null, 5));
    }

    [Fact]
    public void Md5Hash_KnownVector()
    {
        // MD5("123456") = e10adc3949ba59abbe56e057f20f883e
        Assert.Equal("e10adc3949ba59abbe56e057f20f883e", StringHelper.Md5Hash("123456"));
        Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", StringHelper.Md5Hash(""));
    }
}
