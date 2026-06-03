using Portfolio.Domain.Entities;
using Portfolio.Domain.ValueObjects;

namespace Portfolio.Tests.Entities;

public class AssetTests
{
    [Fact]
    public void Constructor_UppercasesSymbol_AndAssignsId()
    {
        var asset = new Asset("aapl", 10m, new Money(150m, "USD"));

        Assert.Equal("AAPL", asset.Symbol);
        Assert.NotEqual(Guid.Empty, asset.Id);
        Assert.Equal(10m, asset.Quantity);
        Assert.Equal(new Money(150m, "USD"), asset.AverageCost);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_RejectsEmptySymbol(string symbol)
    {
        Assert.Throws<ArgumentException>(() =>
            new Asset(symbol, 10m, new Money(150m, "USD")));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_RejectsNonPositiveQuantity(decimal qty)
    {
        Assert.Throws<ArgumentException>(() =>
            new Asset("AAPL", qty, new Money(150m, "USD")));
    }

    [Fact]
    public void IncreasePosition_RecomputesWeightedAverageCost()
    {
        // 10 shares @ $100 + 10 shares @ $200 = 20 shares @ $150 avg
        var asset = new Asset("AAPL", 10m, new Money(100m, "USD"));
        asset.IncreasePosition(10m, new Money(200m, "USD"));

        Assert.Equal(20m, asset.Quantity);
        Assert.Equal(new Money(150m, "USD"), asset.AverageCost);
    }

    [Fact]
    public void IncreasePosition_RejectsCurrencyMismatch()
    {
        var asset = new Asset("AAPL", 10m, new Money(100m, "USD"));

        Assert.Throws<InvalidOperationException>(() =>
            asset.IncreasePosition(5m, new Money(150m, "EUR")));
    }

    [Fact]
    public void DecreasePosition_ReducesQuantity_LeavesAverageCostUnchanged()
    {
        var asset = new Asset("AAPL", 10m, new Money(100m, "USD"));
        asset.DecreasePosition(3m);

        Assert.Equal(7m, asset.Quantity);
        Assert.Equal(new Money(100m, "USD"), asset.AverageCost);
    }

    [Fact]
    public void DecreasePosition_RejectsSellingMoreThanHeld()
    {
        var asset = new Asset("AAPL", 5m, new Money(100m, "USD"));

        Assert.Throws<InvalidOperationException>(() =>
            asset.DecreasePosition(10m));
    }
}