using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserApp.Data;
using UserApp.Models;
using UserApp.Hubs; // Assurez-vous que ce namespace correspond à votre Hub

var builder = WebApplication.CreateBuilder(args);

// --- SERVICES ---
builder.Services.AddControllersWithViews();

// Base de données
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Configuration Identity
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Ajout de SignalR (Indispensable pour le temps réel)
builder.Services.AddSignalR();

var app = builder.Build();

// --- PIPELINE HTTP ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// L'ordre est crucial : Auth avant Authorization
app.UseAuthentication();
app.UseAuthorization();

// Routes des contrôleurs
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Mapping du Hub SignalR
app.MapHub<TicketHub>("/ticketHub");

// --- INITIALISATION (SEEDING) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<Users>>();

    // 1. Création des Rôles
    string[] roles = { "Admin", "Agent", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // 2. Création Admin par défaut
    string adminEmail = "admin@gmail.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var newAdmin = new Users
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Administrator"
        };
        await userManager.CreateAsync(newAdmin, "Admin123");
        await userManager.AddToRoleAsync(newAdmin, "Admin");
    }

    // 3. Création des Catégories
    if (!context.Categories.Any())
    {
        context.Categories.AddRange(
            new Category { Name = "Bug" },
            new Category { Name = "Support" },
            new Category { Name = "Feature Request" }
        );
        await context.SaveChangesAsync();
    }
}

app.Run();