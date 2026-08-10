using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddReverseProxy().LoadFromMemory(
    routes: new[]
    {
            new RouteConfig()
        {
            RouteId = "api",
            ClusterId = "api",
            Match = new RouteMatch {Path = "/api/{**catchall}"},
        }
    },
    clusters: new[]
    {
        new ClusterConfig
        {
            ClusterId = "api",
            Destinations = new Dictionary<string, DestinationConfig>
            {
                ["local"] = new() { Address = "http://localhost:5286" }
            }
        }
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.MapReverseProxy();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();