using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using UIQ;
using UIQ.Services;
using UIQ.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMvc();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IDataBaseService, MySqlDataBaseNcsUiService>();
builder.Services.AddScoped<IDataBaseService, MySqlDataBaseNcsLogService>();
builder.Services.AddScoped<IUserService, UserForDataBaseLoginService>();
builder.Services.AddScoped<IUiqService, UiqService>();

//Login
if (builder.Configuration.GetValue<bool>("IsLoginByOauth"))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["OpenIDConnect:Authority"];
        options.ClientId = builder.Configuration["OpenIDConnect:ClientId"];
        options.ClientSecret = builder.Configuration["OpenIDConnect:ClientSecret"];
        options.ResponseType = "code";

        options.Scope.Add("openid");
        options.Scope.Add("profile");

        options.SaveTokens = true;
    });
}
else
{
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.ExpireTimeSpan = TimeSpan.FromSeconds(1800);
    });
}

builder.Services.AddAuthorization();

builder.Services.Configure<ConnectoinStringOption>(builder.Configuration.GetSection("MySqlOptions").GetSection("ConnectionString"));

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();