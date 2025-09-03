using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Order
{
    [Key]
    public int OrderId { get; set; }
    public required string CustomerName { get; set; }
    public DateTime DatePlaced { get; set; }

    // Navigation property
    public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();

    public void AddItem(InventoryItem item) => Items.Add(item);
    public void RemoveItem(int itemId) => Items.RemoveAll(i => i.ItemId == itemId);
    public string GetOrderSummary() =>
        $"Order #{OrderId} for {CustomerName} | Items: {Items.Count} | Placed: {DatePlaced.ToShortDateString()}";
}