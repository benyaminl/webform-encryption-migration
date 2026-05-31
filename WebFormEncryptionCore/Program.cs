using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<WebFormEncryptionCore.Services.IAuthService, WebFormEncryptionCore.Services.AuthService>();
builder.Services.AddSingleton<WebFormEncryptionCore.Services.IRemoteSessionService, WebFormEncryptionCore.Services.RemoteSessionService>();

// YARP reverse proxy - fallback to WebForm app
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(context =>
    {
        // Strip /WebFormEncryptionApp from Location header on redirects
        context.AddResponseTransform(responseContext =>
        {
            if (responseContext.ProxyResponse?.Headers.Location is { } location)
            {
                var loc = location.ToString();
                if (loc.StartsWith("/WebFormEncryptionApp", StringComparison.OrdinalIgnoreCase))
                {
                    var newLoc = loc["/WebFormEncryptionApp".Length..];
                    if (string.IsNullOrEmpty(newLoc)) newLoc = "/";
                    responseContext.HttpContext.Response.Headers.Location = newLoc;
                }
            }
            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// YARP catch-all: anything not handled by MVC goes to WebForm
app.MapReverseProxy();

app.Run();
