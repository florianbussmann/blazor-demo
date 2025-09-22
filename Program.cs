using blazor_demo.Components;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<BackgroundService>();
builder.Services.AddHangfire(configuration => configuration
        .UseInMemoryStorage());
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/run-background", async (BackgroundService backgroundService) =>
{
    var title = await Task.Run(() => backgroundService.GetText());
    return Results.Ok(new { Title = title });
});

var jobManager = app.Services.GetRequiredService<IRecurringJobManager>();
var serviceProvider = app.Services;

jobManager.AddOrUpdate("background-job", () => serviceProvider.GetRequiredService<BackgroundService>().GetText(), "*/15 5-15 * * *");

app.UseHangfireDashboard();

app.Run();
