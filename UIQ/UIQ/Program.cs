using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.CodeAnalysis.Options;
using UIQ;
using UIQ.Filters;
using UIQ.Services;
using UIQ.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMvc(config =>
{
    config.Filters.Add(new ExceptionFilter());
    config.Filters.Add(new ActoinLogFilter());
    config.Filters.Add(new ResultLogFilter());
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IDataBaseService, MySqlDataBaseNcsUiService>();
builder.Services.AddScoped<IDataBaseService, MySqlDataBaseNcsLogService>();
builder.Services.AddScoped<IUserService, UserForDataBaseLoginService>();
builder.Services.AddScoped<IUiqService, UiqService>();
builder.Services.AddScoped<ISshCommandService, SshCommandService>();
builder.Services.AddScoped<ILogFileService, LogFileService>();
builder.Services.AddScoped<IUploadFileService, UploadFileService>();
builder.Services.AddScoped<IEncodeService, EncodeService>();

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
        options.LoginPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(1800);
        options.SlidingExpiration = true;
    });
}

builder.Services.AddAuthorization();

builder.Services.Configure<ConnectoinStringOption>(builder.Configuration.GetSection("MySqlOptions").GetSection("ConnectionString"));
builder.Services.Configure<RunningJobInfoOption>(builder.Configuration.GetSection("RunningJobInfo"));

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