using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SystemWebAdapters;

var builder = WebApplication.CreateBuilder();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// These must match the data protection settings in MvcApp Startup.Auth.cs for cookie sharing to work
var sharedApplicationName = "CommonMvcAppName";
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetTempPath(), "sharedkeys", sharedApplicationName)))
    .SetApplicationName(sharedApplicationName);

builder.Services.AddAuthentication()
    .AddCookie("SharedCookie", options => options.Cookie.Name = ".AspNet.ApplicationCookie");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSystemWebAdapters()
    .AddHttpApplication<MyApp>()
    .AddHttpModule<MyModule>()
    .AddRemoteAppClient(remote => remote
        .Configure(options =>
        {
            options.RemoteAppUrl = new(builder.Configuration["ReverseProxy:Clusters:fallbackCluster:Destinations:fallbackApp:Address"]);
            options.ApiKey = ClassLibrary.RemoteServiceUtils.ApiKey;
        })
        .AddAuthentication(true)
        .AddSession())
    .AddJsonSessionSerializer(options => ClassLibrary.RemoteServiceUtils.RegisterSessionKeys(options.KnownKeys));

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

app.UseSystemWebAdapters();

app.MapGet("/current-principals-with-metadata", (HttpContext ctx) =>
{
    var user1 = Thread.CurrentPrincipal;
    var user2 = ClaimsPrincipal.Current;

    return "done";
}).WithMetadata(new SetThreadCurrentPrincipalAttribute(), new SingleThreadedRequestAttribute());


app.MapGet("/current-principals-no-metadata", (HttpContext ctx) =>
{
    var user1 = Thread.CurrentPrincipal;
    var user2 = ClaimsPrincipal.Current;

    return "done";
});

app.UseEndpoints(endpoints =>
{
    app.MapDefaultControllerRoute();
    // This method can be used to enable session (or read-only session) on all controllers
    //.RequireSystemWebAdapterSession();

    app.MapReverseProxy();
});

app.Run();

class MyApp : System.Web.HttpApplication
{
    protected void Application_Start()
    {
    }

    protected void Application_BeginRequest()
    {
    }
}

class MyModule : System.Web.IHttpModule
{
    public void Dispose()
    {
    }

    public void Init(System.Web.HttpApplication application)
    {
        application.BeginRequest += (s, e) =>
        {
            var context = ((System.Web.HttpApplication)s!).Context!;
        };

        var handler = new MyHandler();

        application.MapRequestHandler += (s, e) =>
        {
            var context = ((System.Web.HttpApplication)s!).Context!;
            context.Handler = handler;
        };
    }
}

class MyHandler : System.Web.IHttpHandler
{
    public bool IsReusable => throw new NotImplementedException();

    public void ProcessRequest(System.Web.HttpContext context)
    {
        context.Response.Write("Hello");
    }
}
