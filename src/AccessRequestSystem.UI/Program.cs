using AccessRequestSystem.UI.Filters;
using AccessRequestSystem.UI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ApiUnauthorizedExceptionFilter>();
});
builder.Services.AddHttpContextAccessor();

//Cookie-аутентификация: UI не хранит пользователей сам, а лишь кладёт в cookie данные, полученные от API после логина (включая сам JWT — см. AuthTokenHandler)

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

//HttpClient к API с автоподстановкой JWT из cookie
builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Api:BaseUrl не задан в конфигурации.");
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient("ProxyControl")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseProxy = false });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Welcome}/{id?}");

app.Run();
