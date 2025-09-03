using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all inventory endpoints
public class InventoryController : ControllerBase
{
    private readonly LogiTrackContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(LogiTrackContext context, IMemoryCache cache, ILogger<InventoryController> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    // GET: /api/inventory
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItem>>> GetInventory()
    {
        const string cacheKey = "inventory_list";
        var sw = Stopwatch.StartNew();

        if (_cache.TryGetValue(cacheKey, out List<InventoryItem> inventory))
        {
            sw.Stop();
            _logger.LogInformation("Inventory retrieved from cache in {Elapsed} ms", sw.ElapsedMilliseconds);
            return inventory;
        }

        inventory = await _context.InventoryItems
            .AsNoTracking()
            .ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
        };
        _cache.Set(cacheKey, inventory, cacheOptions);

        sw.Stop();
        _logger.LogInformation("Inventory retrieved from DB and cached in {Elapsed} ms", sw.ElapsedMilliseconds);

        return inventory;
    }

    // POST: /api/inventory
    [HttpPost]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<InventoryItem>> AddInventoryItem(InventoryItem item)
    {
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();

        _cache.Remove("inventory_list");
        _logger.LogInformation("Inventory item {ItemName} added by {User}. Cache invalidated.", item.Name, User.Identity?.Name);

        return CreatedAtAction(nameof(GetInventory), new { id = item.ItemId }, item);
    }

    // DELETE: /api/inventory/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> DeleteInventoryItem(int id)
    {
        var item = await _context.InventoryItems.FindAsync(id);
        if (item == null)
        {
            _logger.LogWarning("Attempt to delete non-existent inventory item with ID {Id}", id);
            return NotFound(new { message = $"Inventory item with ID {id} not found." });
        }

        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync();

        _cache.Remove("inventory_list");
        _logger.LogInformation("Inventory item {ItemName} (ID {Id}) deleted by {User}. Cache invalidated.", item.Name, id, User.Identity?.Name);

        return NoContent();
    }
}