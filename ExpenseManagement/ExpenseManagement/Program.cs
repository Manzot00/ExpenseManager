using ExpenseManagement;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Aggiunge per gestire i pagamenti regolari
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<AuthenticationService>();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<IExpenseManagementAPIClient, ExpenseManagementAPIClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7229"); // URL base del progetto API
})
.ConfigureHttpClient((serviceProvider, client) =>
{
    var contextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    var httpContext = contextAccessor.HttpContext;

    if (httpContext != null)
    {
        var path = httpContext.Request.Path;

        // Controlla se la richiesta è per il login o la registrazione
        if (!path.StartsWithSegments("/Home/Login") && !path.StartsWithSegments("/Home/Register"))
        {
            var accessToken = httpContext.Session.GetString("AccessToken");

            // Imposta l'intestazione di autorizzazione solo se è presente un token di accesso
            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
    }
});

// Aggiungi servizi della sessione
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Imposta il timeout della sessione
    options.Cookie.HttpOnly = true; // Rende il cookie della sessione HTTP-only
    options.Cookie.IsEssential = true; // Rende il cookie essenziale
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.LogoutPath = "/Home";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });


builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddHttpContextAccessor();

// Registra il servizio di pagamenti regolari e il servizio di background
builder.Services.AddTransient<IRegularPaymentService, RegularPaymentService>();
builder.Services.AddHostedService<RegularPaymentBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();