using System;
using System.Text.RegularExpressions;
using Kommunist.Core.Helpers;
using Xunit;

namespace Kommunist.Tests.Helpers;

public class EmailTokenGeneratorTests
{
    [Fact]
    public void EncryptForBlobName_Deterministic_WithSamePassword()
    {
        var email = "user@example.com";
        var password = "TestPassword123!";

        var t1 = EmailTokenGenerator.EncryptForBlobName(email, password);
        var t2 = EmailTokenGenerator.EncryptForBlobName(email, password);

        Assert.False(string.IsNullOrWhiteSpace(t1));
        Assert.Equal(t1, t2);
    }

    [Fact]
    public void EncryptForBlobName_DifferentPasswords_ProduceDifferentTokens()
    {
        var email = "user@example.com";

        var t1 = EmailTokenGenerator.EncryptForBlobName(email, "pwd1");
        var t2 = EmailTokenGenerator.EncryptForBlobName(email, "pwd2");

        Assert.NotEqual(t1, t2);
    }

    [Fact]
    public void EncryptForBlobName_DifferentEmails_ProduceDifferentTokens()
    {
        var t1 = EmailTokenGenerator.EncryptForBlobName("user1@example.com", "pwd");
        var t2 = EmailTokenGenerator.EncryptForBlobName("user2@example.com", "pwd");

        Assert.NotEqual(t1, t2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EncryptForBlobName_Throws_OnNullOrEmpty(string? email)
    {
        Assert.Throws<ArgumentException>(() => EmailTokenGenerator.EncryptForBlobName(email!));
    }

    [Fact]
    public void EncryptForBlobName_RemovesUnsafeChars_AndLowercases()
    {
        var token = EmailTokenGenerator.EncryptForBlobName("Upper.CASE+symbols@example.com", "SecretPwd");

        Assert.False(string.IsNullOrWhiteSpace(token));

        // Only lowercase letters and digits are expected after filtering '+', '/', '=' and lowercasing
        Assert.Matches("^[a-z0-9]+$", token);
        Assert.DoesNotContain("+", token);
        Assert.DoesNotContain("/", token);
        Assert.DoesNotContain("=", token);
    }

    [Fact]
    public void EncryptForBlobName_UsesDefaultPassword_WhenNullPassword()
    {
        var email = "abc@example.com";
        var t1 = EmailTokenGenerator.EncryptForBlobName(email, null);
        var t2 = EmailTokenGenerator.EncryptForBlobName(email, null);

        Assert.False(string.IsNullOrWhiteSpace(t1));
        Assert.Equal(t1, t2);
    }
}
