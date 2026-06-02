using Portfolio.Domain.ValueObjects;

namespace Portfolio.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_NormalizesCurrencyToUppercase()
    {
        var money = new Money(10m, "usd");
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Constructor_AllowsNegativeAmounts()
    {
        var money = new Money(-50m, "USD");
        Assert.Equal(-50m, money.Amount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ThrowsWhenCurrencyMissing(string? currency)
    {
        Assert.Throws<ArgumentException>(() => new Money(10m, currency!));
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("U")]
    public void Constructor_ThrowsWhenCurrencyWrongLength(string currency)
    {
        Assert.Throws<ArgumentException>(() => new Money(10m, currency));
    }

    [Fact]
    public void TwoMoniesWithSameValue_AreEqual()
    {
        var a = new Money(100m, "USD");
        var b = new Money(100m, "USD");
        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void TwoMoniesWithDifferentCurrency_AreNotEqual()
    {
        var a = new Money(100m, "USD");
        var b = new Money(100m, "EUR");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void EqualMonies_HaveSameHashCode()
    {
        var a = new Money(100m, "USD");
        var b = new Money(100m, "USD");
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}