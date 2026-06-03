using Portfolio.Domain.ValueObjects;

namespace Portfolio.Domain.Entities;

public class Asset
{
    public Guid Id { get; }
    public string Symbol { get; }
    public decimal Quantity { get; private set; }
    public Money AverageCost { get; private set; }

    public Asset(string symbol, decimal initialQuantity, Money initialCost)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol is required.", nameof(symbol));
        if (initialQuantity <= 0)
            throw new ArgumentException("Initial quantity must be positive.", nameof(initialQuantity));
        ArgumentNullException.ThrowIfNull(initialCost);

        Id = Guid.NewGuid();
        Symbol = symbol.ToUpperInvariant();
        Quantity = initialQuantity;
        AverageCost = initialCost;
    }

    public void IncreasePosition(decimal addedQuantity, Money costPerUnit)
    {
        if (addedQuantity <= 0)
            throw new ArgumentException("Added quantity must be positive.", nameof(addedQuantity));
        ArgumentNullException.ThrowIfNull(costPerUnit);
        if (costPerUnit.Currency != AverageCost.Currency)
            throw new InvalidOperationException(
                $"Cannot mix currencies: asset is {AverageCost.Currency}, addition is {costPerUnit.Currency}.");

        // Weighted-average cost: ((oldQty * oldAvg) + (newQty * newCost)) / totalQty
        var totalOldCost = AverageCost.Amount * Quantity;
        var totalNewCost = costPerUnit.Amount * addedQuantity;
        var newQuantity = Quantity + addedQuantity;
        var newAverageAmount = (totalOldCost + totalNewCost) / newQuantity;

        Quantity = newQuantity;
        AverageCost = new Money(newAverageAmount, AverageCost.Currency);
    }

    public void DecreasePosition(decimal soldQuantity)
    {
        if (soldQuantity <= 0)
            throw new ArgumentException("Sold quantity must be positive.", nameof(soldQuantity));
        if (soldQuantity > Quantity)
            throw new InvalidOperationException(
                $"Cannot sell {soldQuantity} — only {Quantity} held.");

        Quantity -= soldQuantity;
        // AverageCost stays the same on sells — selling doesn't change cost basis.
    }
}