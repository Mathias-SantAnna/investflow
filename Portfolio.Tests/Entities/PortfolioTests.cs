using Portfolio.Domain.ValueObjects;
using PortfolioEntity = Portfolio.Domain.Entities.Portfolio;

namespace Portfolio.Tests.Entities;

public class PortfolioTests
{
    [Fact]
    public void Constructor_SetsAllPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();

        var portfolio = new PortfolioEntity(userId, "My Portfolio", "USD");
        Assert.NotEqual(Guid.Empty, portfolio.Id);
        Assert.Equal(userId, portfolio.UserId);
        Assert.Equal("My Portfolio", portfolio.Name);
        Assert.Equal("USD", portfolio.BaseCurrency);
        Assert.Empty(portfolio.Assets);
        Assert.True(portfolio.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_NormalizesBaseCurrency()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "  usd  ");

        Assert.Equal("USD", portfolio.BaseCurrency);
    }

    [Fact]
    public void Constructor_TrimsName()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "  usd ");
        Assert.Equal("Test", portfolio.Name);
    }

    [Fact]
    public void Constructor_RejectsEmptyUserId()
    {
        Assert.Throws<ArgumentException>(() => new PortfolioEntity(Guid.Empty, "Test", "USD"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_RejectsInvalidBaseCurrency(string baseCurrency)
    {
        Assert.Throws<ArgumentException>(() => new PortfolioEntity(Guid.NewGuid(), "Test", baseCurrency));
    }

        
    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("U$D")]
    [InlineData("1XY")]
    public void Constructor_RejectsBadFormatBaseCurrency(string baseCurrency)
    {
        Assert.Throws<ArgumentException>(() =>
            new PortfolioEntity(Guid.NewGuid(), "Test", baseCurrency));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_RejectsEmptyOrWhitespaceBaseCurrency(string baseCurrency)
    {
        Assert.Throws<ArgumentException>(() => new PortfolioEntity(Guid.NewGuid(), "Test", baseCurrency));
    }

    [Fact]
    public void Constructor_NewPortfolioHasZeroAssets()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");
        Assert.Empty(portfolio.Assets);
    }

    [Fact]
    public void AddAsset_AddsAssetWhenCurrencyMatches()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");

        portfolio.AddAsset("AAPL", 10m, new Money(150m, "USD"));

        Assert.Single(portfolio.Assets);
        Assert.Equal("AAPL", portfolio.Assets[0].Symbol);
        Assert.Equal(10m, portfolio.Assets[0].Quantity);
        Assert.Equal(new Money(150m, "USD"), portfolio.Assets[0].AverageCost);
    }

    [Fact]
    public void AddAsset_RejectNullCost()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");
        Assert.Throws<ArgumentNullException>(() => portfolio.AddAsset("AAPL", 10m, null!));
    }
    
    [Fact]
    public void AddAsset_RejectDuplicateSymbolCaseInsensitive()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");
        portfolio.AddAsset("AAPL", 10m, new Money(150m, "USD"));
        
        Assert.Throws<InvalidOperationException>(() => 
            portfolio.AddAsset("aapl", 10m, new Money(150m, "USD")));
    }

    [Fact]
    public void AddAsset_RejectCurrencyMismatch()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");
        Assert.Throws<InvalidOperationException>(() =>
            portfolio.AddAsset("AAPL", 10m, new Money(150m, "EUR")));    
    }

    [Fact]
    public void AddAsset_RejectWhenAtMAxCount()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");
        for (var i = 0; i < PortfolioEntity.MaxAssetCount; i++)
        {
            portfolio.AddAsset(CreateSymbol(i), 1m, new Money(100m, "USD"));
        }

        Assert.Throws<InvalidOperationException>(() =>
            portfolio.AddAsset("ZZZZ", 1m, new Money(100m, "USD")));
    }

    private static string CreateSymbol(int index)
    {
        var firstLetter = (char)('A' + index / 26);
        var secondLetter = (char)('A' + index % 26);
        
        return $"{firstLetter}{secondLetter}";
    }
    [Fact]
    public void IncreaseAssetPosition_IncreasesExistingAsset()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");

        portfolio.AddAsset("AAPL", 10m, new Money(100m, "USD"));
        var assetId = portfolio.Assets[0].Id;

        portfolio.IncreaseAssetPosition(assetId, 10m, new Money(200m, "USD"));

        Assert.Equal(20m, portfolio.Assets[0].Quantity);
        Assert.Equal(new Money(150m, "USD"), portfolio.Assets[0].AverageCost);
    }

    [Fact]
    public void IncreaseAssetPosition_RejectsCurrencyMismatch()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");

        portfolio.AddAsset("AAPL", 10m, new Money(100m, "USD"));
        var assetId = portfolio.Assets[0].Id;

        Assert.Throws<InvalidOperationException>(() =>
            portfolio.IncreaseAssetPosition(assetId, 5m, new Money(150m, "EUR")));
    }

    [Fact]
    public void IncreaseAssetPosition_RejectsUnknownAssetId()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");

        Assert.Throws<InvalidOperationException>(() =>
            portfolio.IncreaseAssetPosition(Guid.NewGuid(), 5m, new Money(150m, "USD")));
    }

    [Fact]
    public void DecreaseAssetPosition_ReducesQuantity()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");

        portfolio.AddAsset("AAPL", 10m, new Money(100m, "USD"));
        var assetId = portfolio.Assets[0].Id;

        portfolio.DecreaseAssetPosition(assetId, 3m);

        Assert.Equal(7m, portfolio.Assets[0].Quantity);
    }

    [Fact]
    public void DecreaseAssetPosition_RejectsUnknownAssetId()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");

        Assert.Throws<InvalidOperationException>(() =>
            portfolio.DecreaseAssetPosition(Guid.NewGuid(), 3m));
    }

    [Fact]
    public void DecreaseAssetPosition_CanGoToZero()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");

        portfolio.AddAsset("AAPL", 10m, new Money(100m, "USD"));
        var assetId = portfolio.Assets[0].Id;

        portfolio.DecreaseAssetPosition(assetId, 10m);

        Assert.Equal(0m, portfolio.Assets[0].Quantity);
    }

    [Fact]
    public void RemoveAsset_RemovesAsset()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");

        portfolio.AddAsset("AAPL", 10m, new Money(100m, "USD"));
        var assetId = portfolio.Assets[0].Id;

        portfolio.RemoveAsset(assetId);

        Assert.Empty(portfolio.Assets);
    }

    [Fact]
    public void RemoveAsset_RejectsUnknownAssetId()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");

        Assert.Throws<InvalidOperationException>(() =>
            portfolio.RemoveAsset(Guid.NewGuid()));
    }

    [Fact]
    public void RemoveAsset_RejectsEmptyAssetId()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");

        Assert.Throws<ArgumentException>(() => portfolio.RemoveAsset(Guid.Empty));
    }

    [Fact]
    public void Assets_IsReadOnly()
    {
        var portfolio = new PortfolioEntity(Guid.NewGuid(), "Test", "USD");

        portfolio.AddAsset("AAPL", 10m, new Money(100m, "USD"));

        Assert.IsNotType<List<Portfolio.Domain.Entities.Asset>>(portfolio.Assets);

        var listView = Assert.IsAssignableFrom<IList<Portfolio.Domain.Entities.Asset>>(portfolio.Assets);

        Assert.Throws<NotSupportedException>(() =>
            listView.Add(new Portfolio.Domain.Entities.Asset("MSFT", 5m, new Money(200m, "USD"))));
    }
    

}