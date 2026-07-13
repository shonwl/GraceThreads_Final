using GraceThreads.Data;
using GraceThreads.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Secure the Admin folder globally
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
});

builder.Services.AddDistributedMemoryCache();

// 2. Configure Session cookies to Lax to protect PayMongo redirection data
builder.Services.AddSession(options => 
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax; 
});

// Register ApplicationDbContext. Ensure you set DefaultConnection in configuration.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Use ASP.NET Core cookie authentication and role-based authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.LoginPath = "/Index"; // Keeps login cleanly pointed to your Index page
        options.LogoutPath = "/Admin/SignOut";
        options.Cookie.Name = "GraceThreads.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Register ASP.NET Core PasswordHasher for User
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<GraceThreads.Models.User>, Microsoft.AspNetCore.Identity.PasswordHasher<GraceThreads.Models.User>>();

var app = builder.Build();

// Ensure database migrations are applied and seed data is present.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>()?.CreateLogger("DbMigrations");
        logger?.LogError(ex, "Database migration failed");
        if (app.Environment.IsDevelopment()) throw;
    }

    try
    {
        var passwordHasher = scope.ServiceProvider.GetService<Microsoft.AspNetCore.Identity.IPasswordHasher<GraceThreads.Models.User>>();
        DbSeeder.Seed(db, passwordHasher);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>()?.CreateLogger("DbSeeder");
        logger?.LogError(ex, "Database seeding failed");
        if (app.Environment.IsDevelopment()) throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Add Cache-Control Middleware to fix the "Back Button" ghost page issue
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
    context.Response.Headers.Append("Pragma", "no-cache");
    context.Response.Headers.Append("Expires", "-1");
    await next();
});

app.UseStaticFiles();
app.UseRouting();

app.UseSession(); 
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();