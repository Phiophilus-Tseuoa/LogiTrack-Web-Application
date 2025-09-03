using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
/*
class Program
{
    static void Main(string[] args)
    {
        using (var context = new LogiTrackContext())
        {
            // Ensure database exists and migrations are applied
            context.Database.Migrate();

            // 1️⃣ Seed Inventory if empty
            if (!context.InventoryItems.Any())
            {
                var palletJack = new InventoryItem
                {
                    Name = "Pallet Jack",
                    Quantity = 12,
                    Location = "Warehouse A"
                };

                var forklift = new InventoryItem
                {
                    Name = "Forklift",
                    Quantity = 3,
                    Location = "Warehouse B"
                };

                context.InventoryItems.AddRange(palletJack, forklift);
                context.SaveChanges();

                Console.WriteLine("✅ Seeded test inventory items.");
            }

            // 2️⃣ Seed an Order if none exist
            if (!context.Orders.Any())
            {
                var order = new Order
                {
                    CustomerName = "Samir",
                    DatePlaced = DateTime.Now
                };

                // Attach existing inventory items to the order
                var items = context.InventoryItems.Take(2).ToList();
                foreach (var item in items)
                {
                    order.AddItem(item);
                }

                context.Orders.Add(order);
                context.SaveChanges();

                Console.WriteLine("✅ Seeded test order.");
            }

            // 3️⃣ Print Inventory
            Console.WriteLine("\n📦 Current Inventory:");
            foreach (var item in context.InventoryItems)
            {
                item.DisplayInfo();
            }

            // 4️⃣ Print Order Summaries (efficient projection)
            Console.WriteLine("\n📝 Order Summaries:");
            var summaries = context.Orders
                .Select(o => new
                {
                    o.OrderId,
                    o.CustomerName,
                    o.DatePlaced,
                    ItemCount = o.Items.Count
                })
                .ToList();

            foreach (var s in summaries)
            {
                Console.WriteLine($"Order #{s.OrderId} for {s.CustomerName} | Items: {s.ItemCount} | Placed: {s.DatePlaced:d}");
            }
        }

        Console.WriteLine("\n🎯 Data verification complete.");
    }
}*/