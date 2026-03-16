using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PuppyWeightWatcher.Web;
using PuppyWeightWatcher.Web.Components;
using PuppyWeightWatcher.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Azure Key Vault as a configuration source for auth secrets (only when deployed to Azure)
var keyVaultUri = builder.Configuration.GetConnectionString("keyvault");
if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new Azure.Identity.DefaultAzureCredential());
}

// Add Identity database via Aspire SQL Server integration
builder.AddSqlServerDbContext<ApplicationDbContext>("identitydb", configureDbContextOptions: options =>
{
    options.UseSqlServer(sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
    });
});

// Add ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure authentication with external providers
var authBuilder = builder.Services.AddAuthentication();

if (!string.IsNullOrEmpty(builder.Configuration["Authentication:Microsoft:ClientId"]))
{
    authBuilder.AddMicrosoftAccount(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"]!;
    });
}

if (!string.IsNullOrEmpty(builder.Configuration["Authentication:Google:ClientId"]))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });
}

if (!string.IsNullOrEmpty(builder.Configuration["Authentication:GitHub:ClientId"]))
{
    authBuilder.AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
    });
}

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<ViewPreferencesService>();
builder.Services.AddOutputCache();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<UserIdDelegatingHandler>();
builder.Services.AddHttpClient<PuppyApiClient>(client =>
    {
        client.BaseAddress = new("https+http://apiservice");
        client.Timeout = TimeSpan.FromMinutes(2);
    })
    .AddHttpMessageHandler<UserIdDelegatingHandler>();

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Apply pending Identity EF Core migrations on startup
var retryCount = 0;
const int maxRetries = 10;
while (true)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        break;
    }
    catch (Exception ex) when (retryCount < maxRetries)
    {
        retryCount++;
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentityMigration");
        logger.LogWarning(ex, "Identity database migration attempt {Attempt}/{MaxRetries} failed. Retrying in {Delay}s...", retryCount, maxRetries, retryCount * 5);
        await Task.Delay(TimeSpan.FromSeconds(retryCount * 5));
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets().AllowAnonymous();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// External login challenge endpoint
app.MapGet("/Account/ExternalLogin", (string provider, string? returnUrl, SignInManager<ApplicationUser> signInManager) =>
{
    var redirectUrl = $"/Account/ExternalLoginCallback?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}";
    var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
    return Results.Challenge(properties, [provider]);
}).AllowAnonymous();

// Logout endpoint
app.MapPost("/Account/Logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/Account/Login");
});

app.MapDefaultEndpoints();

app.Run();
