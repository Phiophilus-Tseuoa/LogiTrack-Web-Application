using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// --- Standard ASP.NET Core setup ---
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LogiTrackContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<LogiTrackContext>()
.AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// --- Role seeding ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAsync(services);

    // Run your test data seeding here
    RunTestData(services);
}

app.UseAuthentication();
app.UseAuthorization();

// Enable middleware to serve generated Swagger JSON and UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

// --- Methods ---
async Task SeedRolesAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roleNames = { "Manager", "User" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    if (!userManager.Users.Any())
    {
        var adminUser = new ApplicationUser
        {
            UserName = "admin@logitrack.com",
            Email = "admin@logitrack.com",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "AdminPass123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Manager");
        }
    }
}

void RunTestData(IServiceProvider services)
{
    using var context = services.GetRequiredService<LogiTrackContext>();

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

    // 4️⃣ Print Order Summaries
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

    Console.WriteLine("\n🎯 Data verification complete.");
}