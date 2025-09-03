using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all order endpoints
public class OrderController : ControllerBase
{
    private readonly LogiTrackContext _context;
    private readonly ILogger<OrderController> _logger;

    public OrderController(LogiTrackContext context, ILogger<OrderController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: /api/orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        var sw = Stopwatch.StartNew();

        var orders = await _context.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .ToListAsync();

        sw.Stop();
        _logger.LogInformation("Retrieved {Count} orders in {Elapsed} ms", orders.Count, sw.ElapsedMilliseconds);

        return orders;
    }

    // GET: /api/orders/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrderById(int id)
    {
        var sw = Stopwatch.StartNew();

        var order = await _context.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == id);

        sw.Stop();

        if (order == null)
        {
            _logger.LogWarning("Order with ID {Id} not found. Query took {Elapsed} ms", id, sw.ElapsedMilliseconds);
            return NotFound(new { message = $"Order with ID {id} not found." });
        }

        _logger.LogInformation("Retrieved order {Id} with {ItemCount} items in {Elapsed} ms", id, order.Items.Count, sw.ElapsedMilliseconds);
        return order;
    }

    // POST: /api/orders
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(Order order)
    {
        if (order.Items != null && order.Items.Any())
        {
            var itemIds = order.Items.Select(i => i.ItemId).ToList();
            var existingItems = await _context.InventoryItems
                .Where(i => itemIds.Contains(i.ItemId))
                .ToListAsync();

            order.Items = existingItems;
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {Id} created for {Customer} with {ItemCount} items by {User}",
            order.OrderId, order.CustomerName, order.Items.Count, User.Identity?.Name);

        return CreatedAtAction(nameof(GetOrderById), new { id = order.OrderId }, order);
    }

    // DELETE: /api/orders/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            _logger.LogWarning("Attempt to delete non-existent order with ID {Id}", id);
            return NotFound(new { message = $"Order with ID {id} not found." });
        }

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {Id} deleted by {User}", id, User.Identity?.Name);

        return NoContent();
    }
}