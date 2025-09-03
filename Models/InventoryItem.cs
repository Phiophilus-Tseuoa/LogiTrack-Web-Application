using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class InventoryItem
{
    [Key]
    public int ItemId { get; set; }
    public required string Name { get; set; }
    public int Quantity { get; set; }
    public required string Location { get; set; }

    // Foreign key
    public int OrderId { get; set; }

    // Navigation property
    public Order Order { get; set; }

    public void DisplayInfo()
    {
        Console.WriteLine($"Item: {Name} | Quantity: {Quantity} | Location: {Location}");
    }
}