using Portfolio.Domain.ValueObjects;

namespace Portfolio.Domain.Entities;

public class Portfolio
{
    public const int MaxAssetCount = 100;
    private readonly List<Asset> _assets = new();
    public IReadOnlyList<Asset> Assets => _assets.AsReadOnly();
    
    public Guid Id { get; }
    public Guid UserId { get; }
    public string Name { get; }
    public string BaseCurrency { get; }
    public DateTime CreatedAt { get; }

    public Portfolio(Guid userId, string name, string baseCurrency)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID must be specified.", nameof(userId));
        }
        
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }
            
        if (string.IsNullOrWhiteSpace(baseCurrency))
        {
            throw new ArgumentException("Base currency is required.", nameof(baseCurrency));
        }
        
        var normalizedBaseCurrency = baseCurrency.Trim().ToUpperInvariant();

        if (normalizedBaseCurrency.Length != 3 || !normalizedBaseCurrency.All(char.IsLetter))
        {
            throw new ArgumentException(
                "Base currency must be a 3-letter ISO code, for example USD.",
            nameof(baseCurrency));
        }
        
        Id = Guid.NewGuid();
        UserId = userId;
        Name = name.Trim();
        BaseCurrency = normalizedBaseCurrency;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddAsset(string symbol, decimal quantity, Money cost)
    {
        ArgumentNullException.ThrowIfNull(cost);

        if (_assets.Count >= MaxAssetCount)
        {
            throw new InvalidOperationException($"Portfolio cannot contain more than {MaxAssetCount} assets.");
        }
        if (cost.Currency != BaseCurrency)
        {
            throw new InvalidOperationException($"The current asset currency must be '{BaseCurrency}'.");
        }
        
        if (_assets.Any(asset => string.Equals(asset.Symbol, symbol, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Portfolio already contains an asset with the same symbol.");
        }
        
        var asset = new Asset(symbol, quantity, cost);
        _assets.Add(asset);
    }

    public void RemoveAsset(Guid assetId)
    {
        var asset = GetAssetOrThrow(assetId);

        _assets.Remove(asset);
    }

    public void IncreaseAssetPosition(Guid assetId, decimal quantity, Money cost)
    {
        ArgumentNullException.ThrowIfNull(cost);

        if (cost.Currency != BaseCurrency)
        {
            throw new InvalidOperationException(
                $"Cost currency '{cost.Currency}' does not match portfolio base currency '{BaseCurrency}'.");
        }

        var asset = GetAssetOrThrow(assetId);

        asset.IncreasePosition(quantity, cost);
    }

    
    /// Decreases an asset's position. The asset stays in the portfolio even if quantity reaches zero 
    public void DecreaseAssetPosition(Guid assetId, decimal quantity)
    {
        var asset = GetAssetOrThrow(assetId);

        asset.DecreasePosition(quantity);
    }

    private Asset GetAssetOrThrow(Guid assetId)
    {
        if (assetId == Guid.Empty)
            throw new ArgumentException("Asset ID must be specified.", nameof(assetId));

        var asset = _assets.FirstOrDefault(a => a.Id == assetId);
        if (asset is null)
            throw new InvalidOperationException($"Asset '{assetId}' not found in portfolio.");

        return asset;
    }
}